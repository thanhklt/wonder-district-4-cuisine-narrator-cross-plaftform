using SQLite;
using AudioTravelling.Mobile.Data.SQLite.Entities;

namespace AudioTravelling.Mobile.Data.SQLite;

public class AppDbContext
{
    private readonly SQLiteAsyncConnection _db;

    public AppDbContext(string dbPath)
    {
        _db = new SQLiteAsyncConnection(dbPath);
    }

    public async Task InitializeAsync()
    {
        await _db.CreateTableAsync<AppUserCache>();
        await _db.CreateTableAsync<CachedPoi>();
        await _db.CreateTableAsync<CachedPoiImage>();
        await _db.CreateTableAsync<CachedPoiLocalization>();
        await _db.CreateTableAsync<CachedPoiAudio>();
        await _db.CreateTableAsync<PoiGeofenceState>();
        await _db.CreateTableAsync<AudioPlaybackHistory>();
    }

    public SQLiteAsyncConnection Database => _db;
}