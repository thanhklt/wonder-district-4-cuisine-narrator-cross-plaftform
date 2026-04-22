namespace AudioTravelling.API.DTOs;

public record QrCodeResponse(Guid Id, string Code, string QrImageUrl, bool IsActive, DateTime CreatedAt);
public record QrCreateResponse(Guid Id, string Code, string QrImageUrl, bool IsActive);
public record QrToggleResponse(Guid Id, bool IsActive);
