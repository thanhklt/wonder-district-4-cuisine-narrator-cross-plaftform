using Mobile.Models;
using Plugin.Maui.Audio;

namespace Mobile.Services;

public class AudioService
{
    private readonly DatabaseService _db;
    private readonly ApiService _api;
    private IAudioPlayer? _player;
    private int? _currentPoiId;
    private readonly IAudioManager _audioManager;

    public AudioService(DatabaseService db, ApiService api, IAudioManager audioManager)
    {
        _db = db;
        _api = api;
        _audioManager = audioManager;
    }

    public async Task PlayAsync(CachedPoi poi, string langCode, string triggerSource = "geofence")
    {
        Stop();

        var startedAt = DateTime.UtcNow;
        var localPath = await ResolveAudioPathAsync(poi.PoiID, langCode);
        var actualLang = langCode;

        if (localPath is null && langCode != "en")
        {
            localPath = await ResolveAudioPathAsync(poi.PoiID, "en");
            if (localPath is not null) actualLang = "en";
        }

        if (localPath is null)
        {
            await _db.LogPlaybackAsync(new AudioPlaybackHistory
            {
                PoiID = poi.PoiID, LanguageCode = actualLang, TriggerSource = triggerSource,
                StartedAt = startedAt, EndedAt = DateTime.UtcNow,
                WasCompleted = false, WasInterrupted = false
            });
            return;
        }

        try
        {
            _player = _audioManager.CreatePlayer(localPath);
            _currentPoiId = poi.PoiID;

            _player.PlaybackEnded += async (_, _) =>
            {
                await _db.LogPlaybackAsync(new AudioPlaybackHistory
                {
                    PoiID = poi.PoiID, LanguageCode = actualLang, TriggerSource = triggerSource,
                    StartedAt = startedAt, EndedAt = DateTime.UtcNow,
                    WasCompleted = true, WasInterrupted = false
                });
                _currentPoiId = null;
            };
            _player.Play();
        }
        catch
        {
            await _db.LogPlaybackAsync(new AudioPlaybackHistory
            {
                PoiID = poi.PoiID, LanguageCode = actualLang, TriggerSource = triggerSource,
                StartedAt = startedAt, EndedAt = DateTime.UtcNow,
                WasCompleted = false, WasInterrupted = false
            });
        }
    }

    public void Stop()
    {
        if (_player is null) return;
        _player.Stop();
        _player.Dispose();
        _player = null;
        _currentPoiId = null;
    }

    public bool IsPlaying => _player?.IsPlaying ?? false;
    public int? CurrentPoiId => _currentPoiId;

    private async Task<string?> ResolveAudioPathAsync(int poiId, string langCode)
    {
        // 1. File da cache local
        var cached = await _db.GetAudioAsync(poiId, langCode);
        if (cached is not null && !string.IsNullOrEmpty(cached.LocalFilePath)
            && File.Exists(cached.LocalFilePath))
            return cached.LocalFilePath;

        string? downloadedPath = null;

        // 2. Download tu AudioUrl neu da biet (file pre-generated sau khi approve)
        var localization = await _db.GetLocalizationAsync(poiId, langCode);
        if (localization is not null && !string.IsNullOrEmpty(localization.AudioUrl))
            downloadedPath = await _api.DownloadAudioAsync(localization.AudioUrl, poiId, langCode);

        // 3. TTS proxy fallback: goi API de tao audio on-demand
        if (downloadedPath is null)
        {
            var ttsResult = await _api.TtsProxyAsync(poiId, langCode);
            if (ttsResult is not null)
            {
                var dir = Path.Combine(FileSystem.AppDataDirectory, "audio", poiId.ToString());
                Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, $"{langCode}.mp3");
                await File.WriteAllBytesAsync(path, ttsResult.AudioBytes);
                downloadedPath = path;

                // Luu localization vao SQLite ngay lap tuc de description hien dung ngon ngu
                if (!string.IsNullOrEmpty(ttsResult.Description))
                {
                    await _db.SaveLocalizationAsync(new Mobile.Models.CachedPoiLocalization
                    {
                        PoiID        = poiId,
                        LanguageCode = langCode,
                        Name         = ttsResult.Name,
                        Description  = ttsResult.Description,
                        AudioUrl     = ttsResult.AudioUrl,
                        UpdatedDate  = DateTime.UtcNow,
                        CachedAt     = DateTime.UtcNow
                    });
                }
            }
        }

        if (downloadedPath is null) return null;

        // 4. Luu vao SQLite cache (tai su dung record cu de tranh duplicate insert)
        var record = cached ?? new CachedPoiAudio { PoiID = poiId, LanguageCode = langCode };
        record.AudioUrl = localization?.AudioUrl ?? string.Empty;
        record.LocalFilePath = downloadedPath;
        record.DownloadStatus = "downloaded";
        record.CachedAt = DateTime.UtcNow;
        await _db.SaveAudioAsync(record);

        return downloadedPath;
    }
}
