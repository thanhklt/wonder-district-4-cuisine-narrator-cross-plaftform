using AudioTravelling.Mobile.Data.SQLite.Entities;

using SQLite;

namespace AudioTravelling.Mobile.Services.Localization;

public class LocalizationCacheService : ILocalizationCacheService
{
    private readonly SQLiteAsyncConnection _db;

    public LocalizationCacheService(SQLiteAsyncConnection db)
    {
        _db = db;
    }

    public async Task<CachedPoiLocalization?> GetAsync(int poiId, string lang)
    {
        return await _db.Table<CachedPoiLocalization>()
            .FirstOrDefaultAsync(x => x.PoiId == poiId && x.LanguageCode == lang);
    }

    public async Task SaveAsync(CachedPoiLocalization item)
    {
        var existing = await GetAsync(item.PoiId, item.LanguageCode);

        if (existing == null)
        {
            await _db.InsertAsync(item);
            return;
        }

        existing.Name = item.Name;
        existing.Description = item.Description;
        existing.TranslationSource = item.TranslationSource;
        existing.CachedAtUtc = item.CachedAtUtc;

        await _db.UpdateAsync(existing);
    }
}