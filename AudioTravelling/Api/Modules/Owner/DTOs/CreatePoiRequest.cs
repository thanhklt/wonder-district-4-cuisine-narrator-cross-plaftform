namespace Api.Modules.Owner.DTOs;

public class CreatePoiRequest
{
    public string NameVi { get; set; } = string.Empty;
    public string DescriptionVi { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public int PackageId { get; set; }
}