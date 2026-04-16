using AudioTravelling.Mobile.Data.SQLite.Entities;

namespace AudioTravelling.Mobile.Services.Localization;

public interface ILocalizationCacheService
{
    Task<CachedPoiLocalization?> GetAsync(int poiId, string lang);
    Task SaveAsync(CachedPoiLocalization item);
}