namespace AudioTravelling.Mobile.Services.Api.Responses;

public class AudioResponse
{
    public int PoiId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string AudioUrl { get; set; } = string.Empty;
    public string VoiceCode { get; set; } = string.Empty;
    public bool IsGenerated { get; set; }
}