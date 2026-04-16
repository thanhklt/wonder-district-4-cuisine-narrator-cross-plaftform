using SQLite;

namespace AudioTravelling.Mobile.Data.SQLite.Entities;

[Table("PoiGeofenceState")]
public class PoiGeofenceState
{
    [PrimaryKey]
    public int PoiId { get; set; }

    public int IsInsideZone { get; set; }

    public string LastEnterAtUtc { get; set; } = string.Empty;

    public string LastExitAtUtc { get; set; } = string.Empty;

    public string LastTriggeredAtUtc { get; set; } = string.Empty;

    public string LastTriggerType { get; set; } = string.Empty;

    public int ConsecutiveInsideCount { get; set; }

    public string CooldownUntilUtc { get; set; } = string.Empty;

    public string LongCooldownUntilUtc { get; set; } = string.Empty;

    public double LastKnownDistanceMeters { get; set; }

    public string UpdatedAtUtc { get; set; } = string.Empty;
}