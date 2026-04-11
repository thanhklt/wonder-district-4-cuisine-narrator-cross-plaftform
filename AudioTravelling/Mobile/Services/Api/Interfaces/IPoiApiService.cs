using AudioTravelling.Mobile.Services.Api.Responses;

namespace AudioTravelling.Mobile.Services.Api.Interfaces;

public interface IPoiApiService
{
    Task<IEnumerable<PoiResponse>?> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PoiResponse?> GetByIdAsync(string poiId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PoiResponse>?> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task<IEnumerable<PoiResponse>?> GetNearbyAsync(double latitude, double longitude, double radiusKm = 5, CancellationToken cancellationToken = default);
}
