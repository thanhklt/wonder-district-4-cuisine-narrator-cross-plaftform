using System.Net.Http.Json;
using Api.Modules.Localization.DTOs;
using Api.Modules.Localization.Interfaces;

namespace Api.Infrastructure.Services;

public class DeepTranslateClient : IDeepTranslateClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DeepTranslateClient> _logger;

    public DeepTranslateClient(
        HttpClient httpClient,
        ILogger<DeepTranslateClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> TranslateAsync(
        string text,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        if (string.Equals(sourceLanguage, targetLanguage, StringComparison.OrdinalIgnoreCase))
            return text;

        var request = new TranslateRequest
        {
            Text = text,
            SourceLanguage = sourceLanguage,
            TargetLanguage = targetLanguage
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "translate",
                request,
                cancellationToken); 

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Translate service failed: {StatusCode} - {Content}",
                    response.StatusCode, content);
                throw new Exception("Translate service failed.");
            }

            var result = await response.Content.ReadFromJsonAsync<TranslateResponse>(
                cancellationToken: cancellationToken);

            if (result == null || string.IsNullOrWhiteSpace(result.TranslatedText))
                throw new Exception("Translate service returned empty result.");

            return result.TranslatedText;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Translate service timeout.");
            throw new Exception("Translate service timeout.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Cannot connect to translate service.");
            throw new Exception("Cannot connect to translate service.");
        }
    }
}