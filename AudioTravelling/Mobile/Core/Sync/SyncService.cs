using AudioTravelling.Mobile.Data.SQLite.Services;
using AudioTravelling.Mobile.Services.Api.Interfaces;

namespace AudioTravelling.Mobile.Core.Sync;

public class SyncService : ISyncService
{
    private readonly ISyncApiService _syncApiService;
    private readonly ICacheDbService _cacheDbService;

    public SyncService(
        ISyncApiService syncApiService,
        ICacheDbService cacheDbService)
    {
        _syncApiService = syncApiService;
        _cacheDbService = cacheDbService;
    }

    public async Task<SyncResult> SyncBootstrapAsync(
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _cacheDbService.InitializeAsync();

            var response = await _syncApiService.GetBootstrapAsync(languageCode, cancellationToken);
            if (response is null)
            {
                return SyncResult.Success(
                    poiCount: 0,
                    imageCount: 0,
                    localizationCount: 0,
                    audioCount: 0);
            }

            var cachedAtUtc = DateTime.UtcNow.ToString("O");

            var pois = SyncMapper.ToCachedPois(response.Pois, cachedAtUtc);
            var images = SyncMapper.ToCachedPoiImages(response.Pois, cachedAtUtc);
            var localizations = SyncMapper.ToCachedPoiLocalizations(response.Pois, cachedAtUtc);
            var audios = SyncMapper.ToCachedPoiAudios(response.Pois, cachedAtUtc);

            await _cacheDbService.ReplaceAllPoisAsync(
                pois,
                images,
                localizations,
                audios,
                cancellationToken);

            return SyncResult.Success(
                poiCount: pois.Count,
                imageCount: images.Count,
                localizationCount: localizations.Count,
                audioCount: audios.Count);
        }
        catch (OperationCanceledException)
        {
            return SyncResult.Failure("Đồng bộ đã bị hủy.");
        }
        catch (Exception ex)
        {
            return SyncResult.Failure($"Đồng bộ thất bại: {ex.Message}");
        }
    }
}