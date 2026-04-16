namespace AudioTravelling.Mobile.Data.Models;

public class PoiGeofenceState
{
    public int PoiId { get; set; }

    public bool IsInsideZone { get; set; }

    public DateTime? PendingEnterStartedAtUtc { get; set; }

    public DateTime? LastEnterAtUtc { get; set; }
    public DateTime? LastExitAtUtc { get; set; }
    public DateTime? LastTriggeredAtUtc { get; set; }

    public string? LastTriggerType { get; set; }

    public int ConsecutiveInsideCount { get; set; }

    public DateTime? CooldownUntilUtc { get; set; }
    public DateTime? LongCooldownUntilUtc { get; set; }

    public double LastKnownDistanceMeters { get; set; }

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}