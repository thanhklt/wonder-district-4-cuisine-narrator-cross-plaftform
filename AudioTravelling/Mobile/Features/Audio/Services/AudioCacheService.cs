using System.Collections.Concurrent;
using AudioTravelling.Mobile.Data.SQLite.Entities;
using AudioTravelling.Mobile.Data.SQLite.Services;

namespace AudioTravelling.Mobile.Features.Audio.Services;

public class AudioCacheService : IAudioCacheService
{
    private readonly ICacheDbService _cacheDbService;
    private readonly IAudioApiService _audioApiService;
    private readonly HttpClient _downloadClient;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _downloadLocks = new();
    private readonly string _baseUrl;

    public AudioCacheService(
        ICacheDbService cacheDbService, 
        IAudioApiService audioApiService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration) // assuming Configuration is injected, but let's use a static or fallback if not available
    {
        _cacheDbService = cacheDbService;
        _audioApiService = audioApiService;
        _downloadClient = httpClientFactory.CreateClient("DefaultClient");
        
        // Basic fallback for _baseUrl, ideally from configuration or global state
        // For standard local API dev from Android Emulator it's 10.0.2.2:5000
        _baseUrl = "http://10.0.2.2:5000"; // Assuming backend is here
    }

    public async Task<string?> GetOrDownloadAudioAsync(int poiId, string languageCode)
    {
        string lockKey = $"{poiId}_{languageCode}";
        var semaphore = _downloadLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();
        try
        {
            var db = await _cacheDbService.GetDatabaseAsync();

            // 1. Check local DB
            var existing = await db.Table<CachedPoiAudio>()
                .FirstOrDefaultAsync(x => x.PoiId == poiId && x.LanguageCode == languageCode);

            if (existing != null && !string.IsNullOrEmpty(existing.LocalFilePath))
            {
                if (File.Exists(existing.LocalFilePath))
                {
                    return existing.LocalFilePath;
                }
                // File deleted from disk, we need to re-download
            }

            // 2. Fetch from API
            var audioInfo = await _audioApiService.GetPoiAudioAsync(poiId, languageCode);
            if (audioInfo == null || string.IsNullOrEmpty(audioInfo.AudioUrl))
            {
                return null;
            }

            // 3. Download the file
            var fileUrl = audioInfo.AudioUrl;
            if (fileUrl.StartsWith("/"))
            {
                // Make it absolute if it's relative
                fileUrl = $"{_baseUrl}{fileUrl}";
            }

            var downloadResponse = await _downloadClient.GetAsync(fileUrl);
            if (!downloadResponse.IsSuccessStatusCode)
            {
                return null;
            }

            // 4. Save to local storage
            var fileName = $"poi_{poiId}_{languageCode}.mp3";
            var localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            using (var fileStream = File.Create(localPath))
            {
                var stream = await downloadResponse.Content.ReadAsStreamAsync();
                await stream.CopyToAsync(fileStream);
            }

            // 5. Save to SQLite
            if (existing != null)
            {
                existing.LocalFilePath = localPath;
                existing.AudioUrl = audioInfo.AudioUrl;
                existing.VoiceCode = audioInfo.VoiceCode;
                existing.CachedAtUtc = DateTime.UtcNow.ToString("o");
                existing.DownloadStatus = "downloaded";
                await db.UpdateAsync(existing);
            }
            else
            {
                var newCache = new CachedPoiAudio
                {
                    PoiId = poiId,
                    LanguageCode = languageCode,
                    LocalFilePath = localPath,
                    AudioUrl = audioInfo.AudioUrl,
                    VoiceCode = audioInfo.VoiceCode,
                    CachedAtUtc = DateTime.UtcNow.ToString("o"),
                    DownloadStatus = "downloaded"
                };
                await db.InsertAsync(newCache);
            }

            return localPath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Audio download error: {ex.Message}");
            return null;
        }
        finally
        {
            semaphore.Release();
        }
    }
}
