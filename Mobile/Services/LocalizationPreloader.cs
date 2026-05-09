using Mobile.Models;

namespace Mobile.Services;

/// <summary>
/// Preload description + audio file cho tất cả POI theo ngôn ngữ điện thoại.
/// Chạy ngầm sau khi bootstrap/sync xong, không block UI.
/// Khi user vào geofence zone, audio đã sẵn sàng — không cần tải thêm.
/// </summary>
public static class LocalizationPreloader
{
    public static async Task PreloadAsync(
        IEnumerable<int> poiIds,
        string langCode,
        DatabaseService db,
        ApiService api)
    {
        // Tiếng Việt là nguồn gốc — luôn có sẵn, không cần preload
        if (string.IsNullOrEmpty(langCode) || langCode == "vi") return;

        foreach (var poiId in poiIds)
        {
            try
            {
                // Kiểm tra audio cache trước (tốn kém nhất — skip nếu đã có file)
                var cachedAudio = await db.GetAudioAsync(poiId, langCode);
                if (cachedAudio is not null
                    && !string.IsNullOrEmpty(cachedAudio.LocalFilePath)
                    && File.Exists(cachedAudio.LocalFilePath))
                {
                    // Audio đã có sẵn trên máy → skip hoàn toàn
                    continue;
                }

                // Kiểm tra xem localization đã có chưa (để biết có cần gọi TTS không)
                var existingLoc = await db.GetLocalizationAsync(poiId, langCode);

                TtsProxyResult? result = null;

                if (existingLoc is not null && !string.IsNullOrEmpty(existingLoc.AudioUrl))
                {
                    // Localization đã có, chỉ cần tải audio file về
                    var audioPath = await api.DownloadAudioAsync(
                        existingLoc.AudioUrl, poiId, langCode);

                    if (!string.IsNullOrEmpty(audioPath))
                    {
                        await db.SaveAudioAsync(new CachedPoiAudio
                        {
                            PoiID          = poiId,
                            LanguageCode   = langCode,
                            AudioUrl       = existingLoc.AudioUrl,
                            LocalFilePath  = audioPath,
                            DownloadStatus = "downloaded",
                            CachedAt       = DateTime.UtcNow
                        });
                        System.Diagnostics.Debug.WriteLine(
                            $"[Preloader] POI {poiId}/{langCode}: audio downloaded from existing URL.");
                    }
                    continue;
                }

                // Không có localization → gọi TTS proxy để dịch + sinh audio
                result = await api.TtsProxyAsync(poiId, langCode);
                if (result is null) continue;

                // 1. Lưu localization text vào SQLite
                if (!string.IsNullOrEmpty(result.Description))
                {
                    await db.SaveLocalizationAsync(new CachedPoiLocalization
                    {
                        PoiID        = poiId,
                        LanguageCode = langCode,
                        Name         = result.Name,
                        Description  = result.Description,
                        AudioUrl     = result.AudioUrl,
                        UpdatedDate  = DateTime.UtcNow,
                        CachedAt     = DateTime.UtcNow
                    });
                }

                // 2. Lưu audio bytes xuống disk
                if (result.AudioBytes.Length > 0)
                {
                    var dir = Path.Combine(
                        FileSystem.AppDataDirectory, "audio", poiId.ToString());
                    Directory.CreateDirectory(dir);
                    var localPath = Path.Combine(dir, $"{langCode}.mp3");
                    await File.WriteAllBytesAsync(localPath, result.AudioBytes);

                    // 3. Đăng ký vào CachedPoiAudio để AudioService tìm thấy ngay
                    await db.SaveAudioAsync(new CachedPoiAudio
                    {
                        PoiID          = poiId,
                        LanguageCode   = langCode,
                        AudioUrl       = result.AudioUrl,
                        LocalFilePath  = localPath,
                        DownloadStatus = "downloaded",
                        CachedAt       = DateTime.UtcNow
                    });

                    System.Diagnostics.Debug.WriteLine(
                        $"[Preloader] POI {poiId}/{langCode}: TTS generated + saved.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[Preloader] POI {poiId} lang={langCode} error: {ex.Message}");
            }
        }
    }
}
