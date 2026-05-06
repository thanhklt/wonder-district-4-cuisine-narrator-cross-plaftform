using System.Text.Json.Serialization;

namespace Api.DTOs
{
    public class TranslateRequest
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("source_language")]
        public string SourceLanguage { get; set; } = "vi";

        [JsonPropertyName("target_language")]
        public string TargetLanguage { get; set; } = string.Empty;
    }

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
}
