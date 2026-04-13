namespace Api.Modules.Poi.DTOs;

public class PoiDetailResponse
{
    public int PoiId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Radius { get; set; }
    public int Priority { get; set; }
    public List<string> Images { get; set; } = new();
}