namespace AudioTravelling.Mobile.Features.Audio.Models;

public class AudioPlayRequest
{
    public int PoiId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public bool IsManualClick { get; set; }
}
