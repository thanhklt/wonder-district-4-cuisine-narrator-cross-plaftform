namespace AudioTravelling.Mobile.Data.Models;

public class CachedPoi
{
    public int PoiId { get; set; }
    public string NameDefault { get; set; } = string.Empty;
    public string? DescriptionDefault { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int RadiusMeters { get; set; }
    public int Priority { get; set; }
    public string? CoverImageUrl { get; set; }
    public int PackageId { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ServerUpdatedAtUtc { get; set; }
    public DateTime CachedAtUtc { get; set; } = DateTime.UtcNow;
}