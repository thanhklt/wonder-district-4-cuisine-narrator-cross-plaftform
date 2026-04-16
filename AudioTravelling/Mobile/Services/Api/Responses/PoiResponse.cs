namespace AudioTravelling.Mobile.Services.Api.Responses;

public class PoiResponse
{
    public int PoiId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DescriptionVi { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Radius { get; set; }
    public int Priority { get; set; }
    public List<string> Images { get; set; } = new();
    public string? CoverImageUrl { get; set; }
}