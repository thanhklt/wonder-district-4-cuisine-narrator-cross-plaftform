using SQLite;

namespace Mobile.Models;

[Table("CachedAccessSessions")]
public class CachedAccessSession
{
    [PrimaryKey]
    public int SessionID { get; set; }
    public string DeviceID { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiredAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CachedAt { get; set; }

    public bool IsValid => !IsRevoked && ExpiredAt > DateTime.UtcNow;
}
