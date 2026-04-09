using AudioTravelling.Mobile.Features.Order.Models;

namespace AudioTravelling.Mobile.Features.Poi.Models;

// ── POI ──────────────────────────────────────────────────────────────────────
public class PoiModel
{
    public int    Id             { get; set; }
    public string Name          { get; set; } = string.Empty;
    public string Category      { get; set; } = string.Empty;
    public string Description   { get; set; } = string.Empty;
    public string ImageSource   { get; set; } = string.Empty;
    public double DistanceMeters{ get; set; }
    public double Rating        { get; set; }
    public bool   IsOpen        { get; set; }
    public string AudioScript   { get; set; } = string.Empty;

    // Computed display helpers
    public string DistanceLabel => DistanceMeters < 1000
        ? $"{(int)DistanceMeters} m"
        : $"{DistanceMeters / 1000:F1} km";

    public string RatingLabel   => $"★ {Rating:F1}";
    public string StatusLabel   => IsOpen ? "Đang mở cửa" : "Đã đóng cửa";
    public Color  StatusColor   => IsOpen
        ? Color.FromArgb("#FFFFFF")
        : Color.FromArgb("#EF4444");

    public List<FoodItem> Menu  { get; set; } = new();
}
