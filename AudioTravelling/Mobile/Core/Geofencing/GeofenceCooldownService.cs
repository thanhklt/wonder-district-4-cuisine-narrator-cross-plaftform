namespace AudioTravelling.Mobile.Core.Geofencing;

/// <summary>
/// Enforces a cooldown period after a geofence for a specific POI has been triggered,
/// preventing repeated notifications/audio for the same location.
/// </summary>
public class GeofenceCooldownService
{
    private readonly Dictionary<int, DateTime> _cooldowns = new();
    private readonly TimeSpan _cooldownDuration;

    public GeofenceCooldownService(TimeSpan? cooldownDuration = null)
    {
        _cooldownDuration = cooldownDuration ?? TimeSpan.FromMinutes(15);
    }

    /// <summary>
    /// Returns true if the given POI is NOT in cooldown (i.e., can be triggered).
    /// </summary>
    public bool CanTrigger(int poiId)
    {
        if (!_cooldowns.TryGetValue(poiId, out var lastTriggered))
            return true;

        return DateTime.UtcNow - lastTriggered >= _cooldownDuration;
    }

    /// <summary>
    /// Records that the given POI's geofence was triggered, starting the cooldown.
    /// </summary>
    public void RecordTrigger(int poiId)
    {
        _cooldowns[poiId] = DateTime.UtcNow;
    }
}
