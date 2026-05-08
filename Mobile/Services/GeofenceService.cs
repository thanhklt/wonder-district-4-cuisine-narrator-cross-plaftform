using Mobile.Models;

namespace Mobile.Services;

public class GeofenceService
{
    private readonly DatabaseService _db;
    private readonly AudioService _audio;
    private Timer? _timer;
    private Location? _lastLocation;

    private const double AccuracyThreshold = 50.0;
    private const double BufferMeters = 1.0;
    private const int DebounceSeconds = 3;
    private static readonly TimeSpan ShortCooldown = TimeSpan.FromSeconds(45);
    private static readonly TimeSpan LongCooldown = TimeSpan.FromMinutes(15);

    public event Action<CachedPoi>? GeofenceTriggered;

    public GeofenceService(DatabaseService db, AudioService audio)
    {
        _db = db;
        _audio = audio;
    }

    public void Start() =>
        _timer = new Timer(async _ =>
        {
            try { await TickAsync(); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GeofenceService] TickAsync error: {ex}");
            }
        }, null, 0, 5000);

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

    public void UpdateLocation(Location location) => _lastLocation = location;

    private async Task TickAsync()
    {
        if (_lastLocation is null) return;
        if (_lastLocation.Accuracy > AccuracyThreshold) return;

        var pois = await _db.GetActivePoisAsync();
        var now = DateTime.UtcNow;
        var candidates = new List<(CachedPoi poi, double dist)>();

        foreach (var poi in pois)
        {
            var dist = Haversine(_lastLocation.Latitude, _lastLocation.Longitude,
                                 poi.Latitude, poi.Longitude);

            var state = await _db.GetGeofenceStateAsync(poi.PoiID)
                        ?? new PoiGeofenceState { PoiID = poi.PoiID };

            state.LastKnownDistanceMeters = dist;

            var enterThreshold = poi.Radius - BufferMeters;
            var exitThreshold = poi.Radius + BufferMeters;

            if (!state.IsInsideZone && dist <= enterThreshold)
            {
                state.PendingEnterAt ??= now;

                if ((now - state.PendingEnterAt.Value).TotalSeconds >= DebounceSeconds)
                {
                    state.IsInsideZone = true;
                    state.LastEnterAt = now;

                    var shortOk = state.CooldownUntil is null || now >= state.CooldownUntil;
                    var longOk = state.LongCooldownUntil is null || now >= state.LongCooldownUntil;

                    if (shortOk && longOk)
                        candidates.Add((poi, dist));
                }
            }
            else if (state.IsInsideZone && dist >= exitThreshold)
            {
                state.IsInsideZone = false;
                state.LastExitAt = now;
                state.PendingEnterAt = null;
            }
            else if (dist > enterThreshold)
            {
                state.PendingEnterAt = null;
            }

            await _db.SaveGeofenceStateAsync(state);
        }

        if (candidates.Count == 0) return;

        var winner = candidates
            .OrderByDescending(c => pois.First(p => p.PoiID == c.poi.PoiID).Priority)
            .ThenBy(c => c.dist)
            .First().poi;

        var winState = await _db.GetGeofenceStateAsync(winner.PoiID)!;
        winState!.LastTriggeredAt = now;
        winState.CooldownUntil = now + ShortCooldown;
        winState.LongCooldownUntil = now + LongCooldown;
        await _db.SaveGeofenceStateAsync(winState);

        GeofenceTriggered?.Invoke(winner);

        var langCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        await _audio.PlayAsync(winner, langCode, "geofence");
    }

    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
