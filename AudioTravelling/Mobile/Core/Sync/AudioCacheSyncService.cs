using AudioTravelling.Mobile.Core.Abstractions;

namespace AudioTravelling.Mobile.Core.Sync;

/// <summary>
/// Synchronizes audio cache — downloads and manages local audio files
/// based on proximity and user preferences.
/// </summary>
public class AudioCacheSyncService : ISyncService
{
    public bool IsSyncing { get; private set; }
    public DateTime? LastSyncedAt { get; private set; }

    public async Task SyncAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement audio cache sync (download nearby POI audio files)
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
