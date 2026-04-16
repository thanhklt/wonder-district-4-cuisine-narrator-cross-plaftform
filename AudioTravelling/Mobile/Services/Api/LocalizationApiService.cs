using AudioTravelling.Mobile.Services.Api.Interfaces;
using AudioTravelling.Mobile.Services.Api.Responses;

namespace AudioTravelling.Mobile.Services.Api;

public class LocalizationApiService : BaseApiService, ILocalizationApiService
{
    public LocalizationApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<PoiLocalizationResponse?> GetLocalizationAsync(
        int poiId,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<PoiLocalizationResponse>(
            $"api/pois/{poiId}/localizations/{languageCode}",
            cancellationToken);
    }
}