namespace AudioTravelling.Mobile.Services.Auth
{
    public class TokenService : ITokenService
    {
        private const string TokenKey = "access_token";

        public async Task SaveTokenAsync(string token)
        {
            await SecureStorage.SetAsync(TokenKey, token);
        }

        public async Task<string?> GetTokenAsync()
        {
            return await SecureStorage.GetAsync(TokenKey);
        }

        public Task ClearTokenAsync()
        {
            SecureStorage.Remove(TokenKey);
            return Task.CompletedTask;
        }
    }
}
