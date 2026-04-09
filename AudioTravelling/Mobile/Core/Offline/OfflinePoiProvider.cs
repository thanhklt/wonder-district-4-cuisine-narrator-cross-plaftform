using AudioTravelling.Mobile.Features.Poi.Models;

namespace AudioTravelling.Mobile.Core.Offline;

/// <summary>
/// Provides POI data from the local cache when the device is offline.
/// </summary>
public class OfflinePoiProvider
{
    /// <summary>
    /// Returns cached POI data, or an empty list if nothing is cached.
    /// </summary>
    public Task<IReadOnlyList<PoiModel>> GetCachedPoisAsync()
    {
        // TODO: Implement offline POI data retrieval from local SQLite
        throw new NotImplementedException();
    }
}
