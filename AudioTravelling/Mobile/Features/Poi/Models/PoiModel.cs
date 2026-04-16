using AudioTravelling.Mobile.Features.Order.Models;

namespace AudioTravelling.Mobile.Features.Poi.Models;

// ── POI ──────────────────────────────────────────────────────────────────────
public class PoiModel
{
    public int PoiId { get; set; }

    public int OwnerId { get; set; }

    public string NameVi { get; set; } = string.Empty;

    public string DescriptionVi { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public int Radius { get; set; }

    public int Priority { get; set; }

    public int PackageId { get; set; }

    public string ApprovalStatus { get; set; } = "pending";

    public bool IsActive { get; set; }
    public List<string> Images { get; set; } = new();
    public string? CoverImageUrl { get; set; }
}
