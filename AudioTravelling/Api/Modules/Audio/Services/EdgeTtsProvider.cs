using System.Text;
using System.Text.Json;
using Api.Modules.Audio.Interfaces;

namespace Api.Modules.Audio.Services;

public class EdgeTtsProvider : ITtsProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _ttsServiceUrl;

    public EdgeTtsProvider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _ttsServiceUrl = configuration["TtsService:Url"] ?? "http://localhost:8000/tts";
    }

    public async Task<byte[]> SynthesizeAsync(
        string text,
        string languageCode,
        string voiceCode,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            text = text,
            voice = voiceCode,
            lang = languageCode
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(_ttsServiceUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
