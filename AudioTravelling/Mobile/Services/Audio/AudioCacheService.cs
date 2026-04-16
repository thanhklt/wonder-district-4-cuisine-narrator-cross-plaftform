using AudioTravelling.Mobile.Data.Models;
using SQLite;

namespace AudioTravelling.Mobile.Services.Audio;

public class AudioCacheService : IAudioCacheService
{
    private readonly SQLiteAsyncConnection _db;

    public AudioCacheService(SQLiteAsyncConnection db)
    {
        _db = db;
    }

    public async Task<CachedPoiAudio?> GetAsync(int poiId, string lang)
    {
        return await _db.Table<CachedPoiAudio>()
            .FirstOrDefaultAsync(x => x.PoiId == poiId && x.LanguageCode == lang);
    }

    public async Task SaveAsync(CachedPoiAudio item)
    {
        var existing = await GetAsync(item.PoiId, item.LanguageCode);

        if (existing == null)
        {
            await _db.InsertAsync(item);
            return;
        }

        existing.AudioUrl = item.AudioUrl;
        existing.LocalFilePath = item.LocalFilePath;
        existing.VoiceCode = item.VoiceCode;
        existing.DownloadStatus = item.DownloadStatus;
        existing.CachedAtUtc = item.CachedAtUtc;

        await _db.UpdateAsync(existing);
    }
}