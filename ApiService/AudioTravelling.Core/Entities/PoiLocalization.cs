namespace AudioTravelling.Core.Entities;

public class PoiLocalization
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PoiId { get; set; }
    public Poi Poi { get; set; } = null!;
    public string Language { get; set; } = string.Empty;
    public string TextContent { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
