namespace Api.Modules.Owner.DTOs;

public class PoiResponse
{
    public int PoiId { get; set; }
    public string NameVi { get; set; } = string.Empty;
    public string DescriptionVi { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public int Radius { get; set; }
    public int Priority { get; set; }

    public string ApprovalStatus { get; set; } = string.Empty;
}