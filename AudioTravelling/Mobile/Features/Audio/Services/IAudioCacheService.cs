namespace AudioTravelling.Mobile.Features.Audio.Services;

public interface IAudioCacheService
{
    Task<string?> GetOrDownloadAudioAsync(int poiId, string languageCode);
}
