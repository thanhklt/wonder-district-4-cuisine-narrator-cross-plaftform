using AudioTravelling.Mobile.Data.Models;

namespace AudioTravelling.Mobile.Services.Audio;

public interface IAudioCacheService
{
    Task<CachedPoiAudio?> GetAsync(int poiId, string lang);
    Task SaveAsync(CachedPoiAudio item);
}