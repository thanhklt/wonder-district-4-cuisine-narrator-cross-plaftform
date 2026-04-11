using AudioTravelling.Mobile.Services.Api.Interfaces;
using AudioTravelling.Mobile.Services.Api.Requests;
using AudioTravelling.Mobile.Services.Api.Responses;

namespace AudioTravelling.Mobile.Services.Api;

internal class AuthApiService : BaseApiService, IAuthApiService
{
    public AuthApiService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        return await PostAsync<LoginResponse>(ApiEndpoints.Auth_Login, request, cancellationToken);
    }

    public async Task<LoginResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        return await PostAsync<LoginResponse>(ApiEndpoints.Auth_Register, request, cancellationToken);
    }

    public async Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        return await PostAsync<LoginResponse>(ApiEndpoints.Auth_RefreshToken, request, cancellationToken);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await PostAsync<object>(ApiEndpoints.Auth_Logout, cancellationToken: cancellationToken);
    }
}
