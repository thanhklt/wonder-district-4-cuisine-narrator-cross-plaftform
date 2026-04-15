using SQLite;

namespace AudioTravelling.Mobile.Data.SQLite.Entities;

[Table("CachedPoiImages")]
public class CachedPoiImage
{
    [PrimaryKey]
    public int ImageId { get; set; }

    public int PoiId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public string LocalFilePath { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public int IsCover { get; set; }

    public string ServerUpdatedAtUtc { get; set; } = string.Empty;

    public string CachedAtUtc { get; set; } = string.Empty;
}