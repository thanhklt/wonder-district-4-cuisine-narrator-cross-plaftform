namespace AudioTravelling.Mobile.Services.Audio;

public interface IAudioTriggerService
{
    Task TriggerAsync(int poiId, string lang);
    Task TriggerGeofenceAudioAsync(
        int poiId,
        CancellationToken cancellationToken = default);
}