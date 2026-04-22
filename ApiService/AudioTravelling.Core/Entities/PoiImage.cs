namespace AudioTravelling.Core.Entities;

public class PoiImage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PoiId { get; set; }
    public Poi Poi { get; set; } = null!;
    public string ImageUrl { get; set; } = string.Empty;
    public int Order { get; set; }
}
