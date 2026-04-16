using AudioTravelling.Mobile.Services.Api.Responses;

namespace AudioTravelling.Mobile.Services.Api.Interfaces;

public interface IAudioApiService
{
    Task<AudioResponse?> GetPoiAudioAsync(
        int poiId,
        string lang,
        CancellationToken cancellationToken = default);
}