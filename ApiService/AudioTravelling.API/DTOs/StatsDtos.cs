namespace AudioTravelling.API.DTOs;

public record RealtimeResponse(int OnlineCount);
public record SessionStatResponse(string Date, int Count);
public record HeatmapPointResponse(double? Lat, double? Lng);
