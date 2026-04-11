using AudioTravelling.Mobile.Services.Api.Responses;

namespace AudioTravelling.Mobile.Services.Api.Interfaces;

public interface IUserApiService
{
    Task<UserMeResponse?> GetMeAsync(CancellationToken cancellationToken = default);
    Task<UserMeResponse?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserMeResponse>?> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserMeResponse?> UpdateAsync(string userId, UserMeResponse user, CancellationToken cancellationToken = default);
}
