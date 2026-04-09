using AudioTravelling.Mobile.Core.Abstractions;

namespace AudioTravelling.Mobile.Core.Sync;

/// <summary>
/// Synchronizes localization/translation data from a remote source.
/// </summary>
public class LocalizationSyncService : ISyncService
{
    public bool IsSyncing { get; private set; }
    public DateTime? LastSyncedAt { get; private set; }

    public async Task SyncAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement localization sync
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
