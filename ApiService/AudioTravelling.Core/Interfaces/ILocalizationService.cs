namespace AudioTravelling.Core.Interfaces;

public interface ILocalizationService
{
    Task LocalizePoiAsync(Guid poiId, CancellationToken cancellationToken = default);
    Task TranslateOnlyAsync(Guid poiId, CancellationToken cancellationToken = default);
    Task<(int success, int failed)> GenerateAudioBulkAsync(string[] languages, CancellationToken cancellationToken = default);
}
