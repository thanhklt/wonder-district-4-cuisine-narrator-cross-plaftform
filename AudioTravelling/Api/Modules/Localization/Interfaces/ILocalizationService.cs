using Api.Modules.Localization.DTOs;

namespace Api.Modules.Localization.Interfaces;

public interface ILocalizationService
{
    Task<PoiLocalizationResponse> GetOrCreateLocalizationAsync(
        int poiId,
        string languageCode,
        CancellationToken cancellationToken = default);

}