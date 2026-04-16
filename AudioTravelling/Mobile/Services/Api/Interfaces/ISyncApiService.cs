using AudioTravelling.Mobile.Services.Api.Responses;

namespace AudioTravelling.Mobile.Services.Api.Interfaces;

public interface ISyncApiService
{
    Task<SyncBootstrapResponse?> GetBootstrapAsync(
        string languageCode,
        CancellationToken cancellationToken = default);
}