namespace AudioTravelling.Mobile.Core.Geofencing;

public sealed class GeofencePoi
{
    public int PoiId { get; init; }
    public string Name { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public int RadiusMeters { get; init; }
    public int Priority { get; init; }
    public bool IsActive { get; init; }
}