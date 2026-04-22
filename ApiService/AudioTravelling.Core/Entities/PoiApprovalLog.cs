namespace AudioTravelling.Core.Entities;

public class PoiApprovalLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PoiId { get; set; }
    public Poi Poi { get; set; } = null!;
    public Guid AdminId { get; set; }
    public User Admin { get; set; } = null!;
    public string Action { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
