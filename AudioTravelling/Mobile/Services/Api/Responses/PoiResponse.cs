namespace AudioTravelling.Mobile.Services.Api.Responses;

public class PoiResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public double? Rating { get; set; }
    public int ReviewCount { get; set; }
    public string? Address { get; set; }
}
