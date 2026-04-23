namespace AudioTravelling.API.DTOs;

public record MessageResponse(string Message);
public record AudioResponse(string AudioUrl);
public record BulkAudioRequest(string[]? Languages);
public record TtsProxyRequest(string PoiId, string Language, string Text);
