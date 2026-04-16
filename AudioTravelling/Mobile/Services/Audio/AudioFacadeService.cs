using AudioTravelling.Mobile.Core.Helpers;
using AudioTravelling.Mobile.Data.Models;
using AudioTravelling.Mobile.Services.Api.Interfaces;

namespace AudioTravelling.Mobile.Services.Audio;

public class AudioFacadeService
{
    private readonly IAudioApiService _audioApiService;
    private readonly IAudioCacheService _audioCacheService;
    private readonly AudioDownloadService _audioDownloadService;
    private readonly IAudioPlaybackService _audioPlaybackService;

    public AudioFacadeService(
        IAudioApiService audioApiService,
        IAudioCacheService audioCacheService,
        AudioDownloadService audioDownloadService,
        IAudioPlaybackService audioPlaybackService)
    {
        _audioApiService = audioApiService;
        _audioCacheService = audioCacheService;
        _audioDownloadService = audioDownloadService;
        _audioPlaybackService = audioPlaybackService;
    }

    public async Task PlayPoiAudioAsync(
        int poiId,
        string lang,
        CancellationToken cancellationToken = default)
    {
        lang = LanguageHelper.Normalize(lang);

        await _audioPlaybackService.StopAsync();

        var cached = await _audioCacheService.GetAsync(poiId, lang);
        if (cached != null &&
            !string.IsNullOrWhiteSpace(cached.LocalFilePath) &&
            File.Exists(cached.LocalFilePath))
        {
            await _audioPlaybackService.PlayLocalAsync(cached.LocalFilePath, cancellationToken);
            return;
        }

        var apiAudio = await _audioApiService.GetPoiAudioAsync(poiId, lang, cancellationToken);
        if (apiAudio == null || string.IsNullOrWhiteSpace(apiAudio.AudioUrl))
            throw new Exception("Không lấy được audio từ API.");

        await _audioPlaybackService.PlayRemoteAsync(apiAudio.AudioUrl, cancellationToken);

        _ = Task.Run(async () =>
        {
            try
            {
                var localPath = await _audioDownloadService.DownloadAudioAsync(
                    apiAudio.AudioUrl,
                    poiId,
                    lang,
                    cancellationToken);

                await _audioCacheService.SaveAsync(new CachedPoiAudio
                {
                    PoiId = poiId,
                    LanguageCode = lang,
                    AudioUrl = apiAudio.AudioUrl,
                    LocalFilePath = localPath,
                    VoiceCode = apiAudio.VoiceCode,
                    DownloadStatus = "completed",
                    CachedAtUtc = DateTime.UtcNow.ToString("O")
                });
            }
            catch
            {
                // Có thể log sau
            }
        }, cancellationToken);
    }
}