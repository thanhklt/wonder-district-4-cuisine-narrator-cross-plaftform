using AudioTravelling.Mobile.Data.SQLite.Entities;

namespace AudioTravelling.Mobile.Data.SQLite.Services;

public interface ICacheDbService
{
    Task InitializeAsync();

    Task ReplaceAllPoisAsync(
        IEnumerable<CachedPoi> pois,
        IEnumerable<CachedPoiImage> images,
        IEnumerable<CachedPoiLocalization> localizations,
        IEnumerable<CachedPoiAudio> audios,
        CancellationToken cancellationToken = default);

    Task<List<CachedPoi>> GetCachedPoisAsync(CancellationToken cancellationToken = default);
}