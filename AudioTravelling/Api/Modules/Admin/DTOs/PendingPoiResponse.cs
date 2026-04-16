namespace Api.Modules.Admin.DTOs;

public class PendingPoiResponse
{
    public int PoiId { get; set; }
    public int OwnerId { get; set; }
    public string NameVi { get; set; } = string.Empty;
    public string? DescriptionVi { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Radius { get; set; }
    public int Priority { get; set; }
    public int? PackageId { get; set; }
    public string ApprovalStatus { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}