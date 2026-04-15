using System.Text.Json.Serialization;

namespace Api.Modules.Localization.DTOs;

public class TranslateResponse
{
    [JsonPropertyName("translated_text")]
    public string TranslatedText { get; set; } = string.Empty;

    [JsonPropertyName("source_language")]
    public string SourceLanguage { get; set; } = string.Empty;

    [JsonPropertyName("target_language")]
    public string TargetLanguage { get; set; } = string.Empty;

    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;
}