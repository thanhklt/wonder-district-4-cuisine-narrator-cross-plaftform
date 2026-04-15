namespace Api.Modules.Poi.DTOs;

public class PoiListItemResponse
{
    public int PoiId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Radius { get; set; }
    public int Priority { get; set; }
    public string? CoverImageUrl { get; set; }
}