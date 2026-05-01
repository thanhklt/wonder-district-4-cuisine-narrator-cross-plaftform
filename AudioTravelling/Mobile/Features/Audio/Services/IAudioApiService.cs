using AudioTravelling.Mobile.Services.Api.Responses;

namespace AudioTravelling.Mobile.Features.Audio.Services;

public interface IAudioApiService
{
    Task<AudioResponse?> GetPoiAudioAsync(int poiId, string languageCode);
}
