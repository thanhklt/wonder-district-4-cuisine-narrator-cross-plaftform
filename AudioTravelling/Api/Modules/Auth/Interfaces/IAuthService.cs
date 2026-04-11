using Api.Modules.Auth.DTOs;

namespace Api.Modules.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<SignupResponse> SignupAsync(SignupRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<MeResponse?> GetMeAsync(int userId);
    }
}