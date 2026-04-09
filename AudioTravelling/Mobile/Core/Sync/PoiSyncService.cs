using AudioTravelling.Mobile.Core.Abstractions;

namespace AudioTravelling.Mobile.Core.Sync;

/// <summary>
/// Synchronizes POI data from a remote API to the local database.
/// </summary>
public class PoiSyncService : ISyncService
{
    public bool IsSyncing { get; private set; }
    public DateTime? LastSyncedAt { get; private set; }

    public async Task SyncAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement POI data sync from remote API
        IsSyncing = true;
        try
        {
            await Task.Delay(100, cancellationToken); // Placeholder
            LastSyncedAt = DateTime.UtcNow;
        }
        finally
        {
            IsSyncing = false;
        }
    }
}
