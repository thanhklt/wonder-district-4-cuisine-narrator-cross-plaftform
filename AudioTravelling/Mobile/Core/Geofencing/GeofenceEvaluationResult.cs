namespace AudioTravelling.Mobile.Core.Geofencing;

public sealed class GeofenceEvaluationResult
{
    public int PoiId { get; init; }
    public int Priority { get; init; }
    public double DistanceMeters { get; init; }
    public bool IsCandidate { get; init; }
}