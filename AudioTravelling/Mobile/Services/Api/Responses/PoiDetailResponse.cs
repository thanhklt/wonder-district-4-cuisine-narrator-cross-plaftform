namespace AudioTravelling.Mobile.Services.Api.Responses
{
    public class PoiDetailResponse
    {
        public int PoiId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Radius { get; set; }
        public int Priority { get; set; }
        public string? CoverImageUrl { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }
}
