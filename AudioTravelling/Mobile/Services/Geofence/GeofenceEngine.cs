using AudioTravelling.Mobile.Core.Geofencing;
using AudioTravelling.Mobile.Data.Models;
using AudioTravelling.Mobile.Services.Audio;
using AudioTravelling.Mobile.Services.Database;

namespace AudioTravelling.Mobile.Services.Geofence;

public sealed class GeofenceEngine : IGeofenceEngine
{
    private readonly AppDatabase _database;
    private readonly IAudioTriggerService _audioTriggerService;
    private readonly GeofenceEvaluator _evaluator;
    private readonly GeofenceSelector _selector;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public GeofenceEngine(
        AppDatabase database,
        IAudioTriggerService audioTriggerService,
        GeofenceEvaluator evaluator,
        GeofenceSelector selector)
    {
        _database = database;
        _audioTriggerService = audioTriggerService;
        _evaluator = evaluator;
        _selector = selector;
    }

    public async Task ProcessLocationAsync(
        LocationUpdate location,
        CancellationToken cancellationToken = default)
    {
        if (location is null)
            throw new ArgumentNullException(nameof(location));

        if (location.AccuracyMeters > GeofenceConstants.AccuracyRejectThresholdMeters)
            return;

        await _gate.WaitAsync(cancellationToken);

        try
        {
            var now = location.TimestampUtc;

            var cachedPois = await _database.Connection
                .Table<CachedPoi>()
                .Where(x => x.IsActive)
                .ToListAsync();

            if (cachedPois.Count == 0)
                return;

            var results = new List<GeofenceEvaluationResult>();

            foreach (var cachedPoi in cachedPois)
            {
                var poi = new GeofencePoi
                {
                    PoiId = cachedPoi.PoiId,
                    Name = cachedPoi.NameDefault,
                    Latitude = cachedPoi.Latitude,
                    Longitude = cachedPoi.Longitude,
                    RadiusMeters = cachedPoi.RadiusMeters,
                    Priority = cachedPoi.Priority,
                    IsActive = cachedPoi.IsActive
                };

                var state = await _database.Connection
                    .Table<PoiGeofenceState>()
                    .Where(x => x.PoiId == poi.PoiId)
                    .FirstOrDefaultAsync();

                if (state is null)
                {
                    state = new PoiGeofenceState
                    {
                        PoiId = poi.PoiId,
                        IsInsideZone = false,
                        ConsecutiveInsideCount = 0,
                        UpdatedAtUtc = now
                    };

                    await _database.Connection.InsertAsync(state);
                }

                var result = _evaluator.Evaluate(poi, state, location);
                results.Add(result);

                await _database.Connection.UpdateAsync(state);
            }

            var selected = _selector.SelectBest(results);

            if (selected is null)
                return;

            var selectedState = await _database.Connection
                .Table<PoiGeofenceState>()
                .Where(x => x.PoiId == selected.PoiId)
                .FirstOrDefaultAsync();

            if (selectedState is null)
                return;

            selectedState.LastTriggeredAtUtc = now;
            selectedState.LastTriggerType = "geofence";
            selectedState.CooldownUntilUtc = now.AddSeconds(GeofenceConstants.ShortCooldownSeconds);
            selectedState.LongCooldownUntilUtc = now.AddMinutes(GeofenceConstants.LongCooldownMinutes);
            selectedState.UpdatedAtUtc = now;

            await _database.Connection.UpdateAsync(selectedState);

            await _audioTriggerService.TriggerGeofenceAudioAsync(selected.PoiId, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }
}