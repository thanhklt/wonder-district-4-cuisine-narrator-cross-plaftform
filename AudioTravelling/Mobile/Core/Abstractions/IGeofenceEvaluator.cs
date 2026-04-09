namespace AudioTravelling.Mobile.Core.Abstractions;

/// <summary>
/// Evaluates whether the user is within a geofence region for a POI.
/// </summary>
public interface IGeofenceEvaluator
{
    /// <summary>
    /// Determines if the given location falls within the geofence of any registered POI.
    /// </summary>
    /// <param name="latitude">User's current latitude.</param>
    /// <param name="longitude">User's current longitude.</param>
    /// <returns>The ID of the triggered POI, or null if none.</returns>
    Task<int?> EvaluateAsync(double latitude, double longitude);
}
