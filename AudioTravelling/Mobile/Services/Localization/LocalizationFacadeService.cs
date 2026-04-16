using AudioTravelling.Mobile.Core.Helpers;
using AudioTravelling.Mobile.Data.SQLite.Entities;
using AudioTravelling.Mobile.Services.Api.Interfaces;
using AudioTravelling.Mobile.Services.Api.Responses;

namespace AudioTravelling.Mobile.Services.Localization;

public class LocalizationFacadeService
{
    private readonly ILocalizationApiService _localizationApiService;
    private readonly ILocalizationCacheService _localizationCacheService;

    public LocalizationFacadeService(
        ILocalizationApiService localizationApiService,
        ILocalizationCacheService localizationCacheService)
    {
        _localizationApiService = localizationApiService;
        _localizationCacheService = localizationCacheService;
    }

    public async Task<PoiLocalizationResponse> GetLocalizationAsync(
        int poiId,
        string lang,
        CancellationToken cancellationToken = default)
    {
        lang = LanguageHelper.Normalize(lang);

        var cached = await _localizationCacheService.GetAsync(poiId, lang);
        if (cached != null)
        {
            return new PoiLocalizationResponse
            {
                PoiId = cached.PoiId,
                LanguageCode = cached.LanguageCode,
                Name = cached.Name,
                Description = cached.Description,
            };
        }

        var apiResult = await _localizationApiService.GetLocalizationAsync(poiId, lang, cancellationToken);
        if (apiResult == null)
            throw new Exception("Không lấy được localization từ API.");

        await _localizationCacheService.SaveAsync(new CachedPoiLocalization
        {
            PoiId = apiResult.PoiId,
            LanguageCode = apiResult.LanguageCode,
            Name = apiResult.Name,
            Description = apiResult.Description,
            CachedAtUtc = DateTime.UtcNow.ToString("O")
        });

        return apiResult;
    }
}