using AudioTravelling.Mobile.Data.SQLite.Entities;
using SQLite;

namespace AudioTravelling.Mobile.Data.SQLite.Services;

public class CacheDbService : ICacheDbService
{
    private readonly AppDbContext _dbContext;
    private SQLiteAsyncConnection Database => _dbContext.Database;

    public CacheDbService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync()
    {
        await _dbContext.InitializeAsync();
    }

    public async Task ReplaceAllPoisAsync(
        IEnumerable<CachedPoi> pois,
        IEnumerable<CachedPoiImage> images,
        IEnumerable<CachedPoiLocalization> localizations,
        IEnumerable<CachedPoiAudio> audios,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await Database.RunInTransactionAsync(connection =>
        {
            connection.DeleteAll<CachedPoiAudio>();
            connection.DeleteAll<CachedPoiLocalization>();
            connection.DeleteAll<CachedPoiImage>();
            connection.DeleteAll<CachedPoi>();

            connection.InsertAll(pois);
            connection.InsertAll(images);
            connection.InsertAll(localizations);
            connection.InsertAll(audios);
        });
    }

    public async Task<List<CachedPoi>> GetCachedPoisAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await Database.Table<CachedPoi>()
            .Where(x => x.IsActive == 1)
            .OrderByDescending(x => x.Priority)
            .ToListAsync();
    }
}