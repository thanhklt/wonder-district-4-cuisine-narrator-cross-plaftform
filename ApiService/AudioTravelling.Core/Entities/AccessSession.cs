namespace AudioTravelling.Core.Entities;

public class AccessSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AccessCodeId { get; set; }
    public AccessCode AccessCode { get; set; } = null!;
    public string SessionToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? DeviceInfo { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
}
