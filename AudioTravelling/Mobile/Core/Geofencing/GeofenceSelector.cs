namespace AudioTravelling.Mobile.Core.Geofencing;

public sealed class GeofenceSelector
{
    public GeofenceEvaluationResult? SelectBest(IEnumerable<GeofenceEvaluationResult> results)
    {
        return results
            .Where(x => x.IsCandidate)
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.DistanceMeters)
            .FirstOrDefault();
    }
}