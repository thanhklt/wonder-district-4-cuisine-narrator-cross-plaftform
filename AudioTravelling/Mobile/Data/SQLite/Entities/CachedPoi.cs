using SQLite;

namespace AudioTravelling.Mobile.Data.SQLite.Entities;

[Table("CachedPois")]
public class CachedPoi
{
    [PrimaryKey]
    public int PoiId { get; set; }

    public string NameDefault { get; set; } = string.Empty;

    public string DescriptionDefault { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public int RadiusMeters { get; set; }

    public int Priority { get; set; }

    public string CoverImageUrl { get; set; } = string.Empty;

    public int PackageId { get; set; }

    public int IsActive { get; set; }

    public string ServerUpdatedAtUtc { get; set; } = string.Empty;

    public string CachedAtUtc { get; set; } = string.Empty;
}