namespace Api.Modules.Localization.DTOs;

public class PoiLocalizationResponse
{
    public int PoiId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}