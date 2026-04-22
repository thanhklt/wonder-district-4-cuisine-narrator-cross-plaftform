using AudioTravelling.Core.Enums;

namespace AudioTravelling.API.DTOs;

public record CreatePoiRequest(string Name, double Lat, double Lng, int PackageId, string Description);
public record UpdatePoiRequest(string Name, double Lat, double Lng, string Description);
public record RejectRequest(string? Note);
public record PoiStatusResponse(Guid Id, PoiStatus Status);
public record LocalizationDto(string Language, string? TextContent, string? AudioUrl);
public record PoiSummaryResponse(
    Guid Id, string Name, double Lat, double Lng, int RadiusMeters, int Priority,
    IEnumerable<string> Images,
    IEnumerable<LocalizationDto> Localizations);
