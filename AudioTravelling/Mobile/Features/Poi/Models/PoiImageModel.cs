namespace AudioTravelling.Mobile.Features.Poi.Models;

public class PoiImageModel
{
    public string ImageId { get; set; } = string.Empty;
    public int PoiId { get; set; }

    public string? ImageUrl { get; set; }      // từ API / DB
    public string? LocalFilePath { get; set; } // dùng cho offline

    public int DisplayOrder { get; set; }
    public bool IsCover { get; set; }

    // QUAN TRỌNG NHẤT
    public string DisplaySource
    {
        get
        {
            // ưu tiên local
            if (!string.IsNullOrWhiteSpace(LocalFilePath) && File.Exists(LocalFilePath))
                return LocalFilePath;

            // fallback online
            if (!string.IsNullOrWhiteSpace(ImageUrl))
                return ImageUrl;

            // fallback cuối
            return "placeholder_poi.png";
        }
    }
}