using System.Text.Json.Serialization;

namespace Api.Modules.Localization.DTOs;

public class TranslateRequest
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("source_language")]
    public string SourceLanguage { get; set; } = "vi";

    [JsonPropertyName("target_language")]
    public string TargetLanguage { get; set; } = string.Empty;
}