using AudioTravelling.Mobile.Services.Api.Requests;
using AudioTravelling.Mobile.Services.Api.Responses;

namespace AudioTravelling.Mobile.Services.Api.Interfaces;

public interface IAuthApiService
{
    Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<RegisterResponse?> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<LoginResponse?> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        CancellationToken cancellationToken = default);
}