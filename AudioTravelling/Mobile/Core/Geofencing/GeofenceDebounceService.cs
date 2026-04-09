namespace AudioTravelling.Mobile.Core.Geofencing;

/// <summary>
/// Prevents rapid-fire geofence triggers by debouncing enter/exit events.
/// </summary>
public class GeofenceDebounceService
{
    private readonly TimeSpan _debounceInterval;
    private DateTime _lastTriggerTime = DateTime.MinValue;

    public GeofenceDebounceService(TimeSpan? debounceInterval = null)
    {
        _debounceInterval = debounceInterval ?? TimeSpan.FromSeconds(5);
    }

    /// <summary>
    /// Returns true if the trigger should be processed (not debounced).
    /// </summary>
    public bool ShouldTrigger()
    {
        var now = DateTime.UtcNow;
        if (now - _lastTriggerTime < _debounceInterval)
            return false;

        _lastTriggerTime = now;
        return true;
    }
}
