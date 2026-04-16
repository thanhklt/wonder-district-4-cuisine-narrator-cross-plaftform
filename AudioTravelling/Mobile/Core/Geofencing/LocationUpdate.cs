namespace AudioTravelling.Mobile.Core.Geofencing;

public sealed class LocationUpdate
{
    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public double AccuracyMeters { get; init; }

    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
}