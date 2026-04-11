using AudioTravelling.Mobile.Services.Api.Interfaces;
using AudioTravelling.Mobile.Services.Api.Responses;

namespace AudioTravelling.Mobile.Services.Api;

internal class UserApiService : BaseApiService, IUserApiService
{
    public UserApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<UserMeResponse?> GetMeAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<UserMeResponse>(ApiEndpoints.User_Me, cancellationToken);
    }

    public async Task<UserMeResponse?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var endpoint = string.Format(ApiEndpoints.User_GetById, userId);
        return await GetAsync<UserMeResponse>(endpoint, cancellationToken);
    }

    public async Task<IEnumerable<UserMeResponse>?> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<IEnumerable<UserMeResponse>>(ApiEndpoints.User_GetAll, cancellationToken);
    }

    public async Task<UserMeResponse?> UpdateAsync(string userId, UserMeResponse user, CancellationToken cancellationToken = default)
    {
        var endpoint = string.Format(ApiEndpoints.User_Update, userId);
        return await PutAsync<UserMeResponse>(endpoint, user, cancellationToken);
    }
}
