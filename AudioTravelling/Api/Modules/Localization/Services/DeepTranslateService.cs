using System.Net.Http.Json;
using Api.Modules.Localization.Interfaces;

namespace Api.Modules.Localization.Services;

public class DeepTranslateService : IDeepTranslateClient
{
    private readonly HttpClient _httpClient;

    public DeepTranslateService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> TranslateAsync(
        string text,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var request = new TranslateRequest
        {
            Text = text,
            SourceLanguage = sourceLanguage,
            TargetLanguage = targetLanguage
        };

        var response = await _httpClient.PostAsJsonAsync("translate", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TranslateResponse>(cancellationToken: cancellationToken);

        return result?.TranslatedText ?? text;
    }

    private sealed class TranslateRequest
    {
        public string Text { get; set; } = string.Empty;
        public string SourceLanguage { get; set; } = string.Empty;
        public string TargetLanguage { get; set; } = string.Empty;
    }

    private sealed class TranslateResponse
    {
        public string TranslatedText { get; set; } = string.Empty;
    }
}