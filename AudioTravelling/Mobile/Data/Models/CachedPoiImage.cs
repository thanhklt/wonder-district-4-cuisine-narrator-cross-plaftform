namespace AudioTravelling.Mobile.Data.Models;

public class CachedPoiImage
{
    public string ImageId { get; set; } = string.Empty;
    public int PoiId { get; set; }
    public string? ImageUrl { get; set; }
    public string? LocalFilePath { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsCover { get; set; }
}