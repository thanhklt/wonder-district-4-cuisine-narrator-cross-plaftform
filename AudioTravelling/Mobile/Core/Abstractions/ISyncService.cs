namespace AudioTravelling.Mobile.Core.Abstractions;

/// <summary>
/// Generic sync service interface for synchronizing local data with a remote source.
/// </summary>
public interface ISyncService
{
    /// <summary>Perform a full sync cycle.</summary>
    Task SyncAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns true if a sync is currently in progress.</summary>
    bool IsSyncing { get; }

    /// <summary>The timestamp of the last successful sync.</summary>
    DateTime? LastSyncedAt { get; }
}
