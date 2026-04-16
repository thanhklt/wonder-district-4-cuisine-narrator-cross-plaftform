using AudioTravelling.Mobile.Services.Api.Exceptions;
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

        try
        {
            return await GetAsync<SyncBootstrapResponse>(endpoint, cancellationToken);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }
}