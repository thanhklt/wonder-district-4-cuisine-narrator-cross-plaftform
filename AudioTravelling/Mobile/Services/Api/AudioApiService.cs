using AudioTravelling.Mobile.Services.Api.Interfaces;
using AudioTravelling.Mobile.Services.Api.Responses;

namespace AudioTravelling.Mobile.Services.Api;

public class AudioApiService : BaseApiService, IAudioApiService
{
    public AudioApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<AudioResponse?> GetPoiAudioAsync(
        int poiId,
        string lang,
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<AudioResponse>(
            $"api/pois/{poiId}/audio?lang={Uri.EscapeDataString(lang)}",
            cancellationToken);
    }
}