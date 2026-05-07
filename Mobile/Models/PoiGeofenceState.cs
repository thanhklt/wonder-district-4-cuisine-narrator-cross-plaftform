using SQLite;

namespace Mobile.Models;

[Table("PoiGeofenceState")]
public class PoiGeofenceState
{
    [PrimaryKey]
    public int PoiID { get; set; }
    public bool IsInsideZone { get; set; }
    public DateTime? LastEnterAt { get; set; }
    public DateTime? LastExitAt { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
    public int ConsecutiveInsideCount { get; set; }
    public DateTime? PendingEnterAt { get; set; }
    public DateTime? CooldownUntil { get; set; }
    public DateTime? LongCooldownUntil { get; set; }
    public double LastKnownDistanceMeters { get; set; }
    public DateTime UpdatedAt { get; set; }
}
