using AudioTravelling.Core.Enums;

namespace AudioTravelling.Core.Entities;

public class Poi
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lng { get; set; }
    public int RadiusMeters { get; set; }
    public int Priority { get; set; }
    public PoiStatus Status { get; set; } = PoiStatus.Draft;
    public int PackageId { get; set; }
    public Package Package { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<PoiImage> Images { get; set; } = [];
    public ICollection<PoiLocalization> Localizations { get; set; } = [];
    public ICollection<PoiApprovalLog> ApprovalLogs { get; set; } = [];
}
