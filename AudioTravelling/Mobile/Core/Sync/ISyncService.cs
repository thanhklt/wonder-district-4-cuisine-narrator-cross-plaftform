namespace AudioTravelling.Mobile.Core.Sync;

public interface ISyncService
{
    Task<SyncResult> SyncBootstrapAsync(
        string languageCode,
        CancellationToken cancellationToken = default);
}