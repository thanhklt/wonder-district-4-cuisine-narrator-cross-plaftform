namespace AudioTravelling.API.DTOs;

public record PackageRequest(string Name, int RadiusMeters, int Priority, decimal Price, string? Description);
public record PackageResponse(int Id, string Name, int RadiusMeters, int Priority, decimal Price, string? Description);
