using AudioTravelling.Mobile.Services.Api.Responses;

namespace AudioTravelling.Mobile.Services.Api.Interfaces;

public interface ILocalizationApiService
{
    Task<PoiLocalizationResponse?> GetLocalizationAsync(
        int poiId,
        string languageCode,
        CancellationToken cancellationToken = default);
}