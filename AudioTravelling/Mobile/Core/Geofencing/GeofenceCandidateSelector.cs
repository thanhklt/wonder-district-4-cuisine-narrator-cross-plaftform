namespace AudioTravelling.Mobile.Core.Geofencing;

/// <summary>
/// Selects the best candidate POI when multiple geofences overlap.
/// Uses proximity-based ranking.
/// </summary>
public class GeofenceCandidateSelector
{
    /// <summary>
    /// Given a list of candidate POI IDs and their distances, returns the best candidate.
    /// </summary>
    public int? SelectBestCandidate(IReadOnlyList<(int PoiId, double DistanceMeters)> candidates)
    {
        // TODO: Implement candidate selection logic (closest, priority-based, etc.)
        throw new NotImplementedException();
    }
}
