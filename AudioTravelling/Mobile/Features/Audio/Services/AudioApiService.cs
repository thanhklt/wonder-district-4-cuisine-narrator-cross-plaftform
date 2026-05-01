using System.Net.Http.Json;
using AudioTravelling.Mobile.Services.Api.Responses;
using AudioTravelling.Mobile.Services.Auth;

namespace AudioTravelling.Mobile.Features.Audio.Services;

public class AudioApiService : IAudioApiService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthCacheService _authCacheService;

    public AudioApiService(IHttpClientFactory httpClientFactory, IAuthCacheService authCacheService)
    {
        _httpClient = httpClientFactory.CreateClient("AuthenticatedClient");
        _authCacheService = authCacheService;
    }

    public async Task<AudioResponse?> GetPoiAudioAsync(int poiId, string languageCode)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/pois/{poiId}/audio?lang={languageCode}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AudioResponse>();
            }
            
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching audio info: {ex.Message}");
            return null;
        }
    }
}
