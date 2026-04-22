namespace AudioTravelling.Core.Interfaces;

public interface ILocalizationService
{
    Task LocalizePoiAsync(Guid poiId, CancellationToken cancellationToken = default);
}
