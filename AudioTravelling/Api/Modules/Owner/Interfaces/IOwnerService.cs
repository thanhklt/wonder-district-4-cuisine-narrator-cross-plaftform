namespace Api.Modules.Owner.Interfaces;

using Api.Modules.Auth.DTOs;
using Api.Modules.Owner.DTOs;

public interface IOwnerService
{
    Task<LoginResponse> SignupAsync(OwnerSignupRequest request);
}