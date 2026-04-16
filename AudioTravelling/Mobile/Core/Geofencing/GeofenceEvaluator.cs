using AudioTravelling.Mobile.Core.Helper;
using AudioTravelling.Mobile.Data.Models;

namespace AudioTravelling.Mobile.Core.Geofencing;

public sealed class GeofenceEvaluator
{
    public GeofenceEvaluationResult Evaluate(
        GeofencePoi poi,
        PoiGeofenceState state,
        LocationUpdate location)
    {
        var now = location.TimestampUtc;

        var distance = GeoDistanceHelper.CalculateMeters(
            location.Latitude,
            location.Longitude,
            poi.Latitude,
            poi.Longitude);

        state.LastKnownDistanceMeters = distance;
        state.UpdatedAtUtc = now;

        var enterThreshold = Math.Max(0, poi.RadiusMeters - GeofenceConstants.BufferMeters);
        var exitThreshold = poi.RadiusMeters + GeofenceConstants.BufferMeters;

        if (!state.IsInsideZone)
        {
            if (distance <= enterThreshold)
            {
                state.ConsecutiveInsideCount++;

                if (!state.PendingEnterStartedAtUtc.HasValue)
                {
                    state.PendingEnterStartedAtUtc = now;
                }
                else if ((now - state.PendingEnterStartedAtUtc.Value).TotalSeconds >= GeofenceConstants.DebounceSeconds)
                {
                    state.IsInsideZone = true;
                    state.LastEnterAtUtc = now;
                    state.PendingEnterStartedAtUtc = null;
                }
            }
            else
            {
                ResetPending(state);
            }
        }
        else
        {
            if (distance >= exitThreshold)
            {
                state.IsInsideZone = false;
                state.LastExitAtUtc = now;
                ResetPending(state);
            }
            else
            {
                // vẫn ở trong vùng thì bỏ pending enter cũ đi
                state.PendingEnterStartedAtUtc = null;
            }
        }

        var canTrigger =
            poi.IsActive &&
            state.IsInsideZone &&
            state.LastEnterAtUtc.HasValue &&
            !IsInCooldown(state, now) &&
            !AlreadyTriggeredInCurrentEnter(state);

        return new GeofenceEvaluationResult
        {
            PoiId = poi.PoiId,
            Priority = poi.Priority,
            DistanceMeters = distance,
            IsCandidate = canTrigger
        };
    }

    private static bool IsInCooldown(PoiGeofenceState state, DateTime now)
    {
        var inShortCooldown =
            state.CooldownUntilUtc.HasValue &&
            now < state.CooldownUntilUtc.Value;

        var inLongCooldown =
            state.LongCooldownUntilUtc.HasValue &&
            now < state.LongCooldownUntilUtc.Value;

        return inShortCooldown || inLongCooldown;
    }

    private static bool AlreadyTriggeredInCurrentEnter(PoiGeofenceState state)
    {
        return state.LastTriggeredAtUtc.HasValue &&
               state.LastEnterAtUtc.HasValue &&
               state.LastTriggeredAtUtc.Value >= state.LastEnterAtUtc.Value;
    }

    private static void ResetPending(PoiGeofenceState state)
    {
        state.PendingEnterStartedAtUtc = null;
        state.ConsecutiveInsideCount = 0;
    }
}