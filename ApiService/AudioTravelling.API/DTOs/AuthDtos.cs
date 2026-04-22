namespace AudioTravelling.API.DTOs;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Role, Guid UserId);
