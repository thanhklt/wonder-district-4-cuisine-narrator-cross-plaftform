namespace AudioTravelling.Core.Entities;

public class Package
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RadiusMeters { get; set; }
    public int Priority { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public ICollection<Poi> Pois { get; set; } = [];
}
