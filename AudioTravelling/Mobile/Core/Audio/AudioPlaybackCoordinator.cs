namespace AudioTravelling.Mobile.Core.Audio;

/// <summary>
/// Coordinates audio playback across features — decides when to start, pause, or preempt audio
/// based on geofence events, user actions, and queue state.
/// </summary>
public class AudioPlaybackCoordinator
{
    /// <summary>
    /// Handles a new geofence trigger by coordinating with the playback queue.
    /// </summary>
    public Task OnGeofenceTriggeredAsync(int poiId, string audioFilePath)
    {
        // TODO: Implement coordination logic (interrupt current? queue? etc.)
        throw new NotImplementedException();
    }
}
