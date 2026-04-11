
namespace AudioTravelling.Mobile.Services.Auth
{
    public interface ITokenService
    {
        Task SaveTokenAsync(string token);
        Task<string?> GetTokenAsync();
        Task ClearTokenAsync();
    }
}
