using AudioTravelling.Mobile.Core.Abstractions;

namespace AudioTravelling.Mobile.Core.Geofencing;

/// <summary>
/// Evaluates user proximity to POI geofences based on GPS coordinates.
/// </summary>
public class GeofenceEvaluator : IGeofenceEvaluator
{
    private const double DefaultRadiusMeters = 100.0;

    public Task<int?> EvaluateAsync(double latitude, double longitude)
    {
        // TODO: Implement real geofence evaluation against registered POI coordinates.
        throw new NotImplementedException();
    }
}
