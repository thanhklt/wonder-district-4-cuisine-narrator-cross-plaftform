using AudioTravelling.Mobile.Services.Api.Interfaces;
using AudioTravelling.Mobile.Services.Api.Responses;

namespace AudioTravelling.Mobile.Services.Api;

internal class PoiApiService : BaseApiService, IPoiApiService
{
    public PoiApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<IEnumerable<PoiResponse>?> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<IEnumerable<PoiResponse>>(ApiEndpoints.Poi_GetAll, cancellationToken);
    }

    public async Task<PoiResponse?> GetByIdAsync(string poiId, CancellationToken cancellationToken = default)
    {
        var endpoint = string.Format(ApiEndpoints.Poi_GetById, poiId);
        return await GetAsync<PoiResponse>(endpoint, cancellationToken);
    }

    public async Task<IEnumerable<PoiResponse>?> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var endpoint = $"{ApiEndpoints.Poi_Search}?q={Uri.EscapeDataString(query)}";
        return await GetAsync<IEnumerable<PoiResponse>>(endpoint, cancellationToken);
    }

    public async Task<IEnumerable<PoiResponse>?> GetNearbyAsync(double latitude, double longitude, double radiusKm = 5, CancellationToken cancellationToken = default)
    {
        var endpoint = $"{ApiEndpoints.Poi_GetNearby}?latitude={latitude}&longitude={longitude}&radiusKm={radiusKm}";
        return await GetAsync<IEnumerable<PoiResponse>>(endpoint, cancellationToken);
    }
}
