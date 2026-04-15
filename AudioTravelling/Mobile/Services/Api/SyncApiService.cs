using AudioTravelling.Mobile.Services.Api.Interfaces;
using AudioTravelling.Mobile.Services.Api.Responses;

namespace AudioTravelling.Mobile.Services.Api;

public class SyncApiService : BaseApiService, ISyncApiService
{
    public SyncApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<SyncBootstrapResponse?> GetBootstrapAsync(
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        var endpoint = $"api/sync/bootstrap?lang={Uri.EscapeDataString(languageCode)}";
        return await GetAsync<SyncBootstrapResponse>(endpoint, cancellationToken);
    }
}