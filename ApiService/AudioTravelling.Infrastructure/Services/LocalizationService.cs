using System.Net.Http.Json;
using AudioTravelling.Core.Entities;
using AudioTravelling.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AudioTravelling.Infrastructure.Services;

public class LocalizationService(
    IAppDbContext db,
    IHttpClientFactory httpClientFactory,
    IConfiguration config,
    ILogger<LocalizationService> logger) : ILocalizationService
{
    private static readonly string[] DefaultLanguages = ["en", "zh", "ja", "ru"];

    public async Task LocalizePoiAsync(Guid poiId, CancellationToken cancellationToken = default)
    {
        var poi = await db.Pois
            .Include(p => p.Localizations)
            .FirstOrDefaultAsync(p => p.Id == poiId, cancellationToken);

        if (poi is null) return;

        var viLocalization = poi.Localizations.FirstOrDefault(l => l.Language == "vi");
        if (viLocalization is null)
        {
            logger.LogWarning("POI {PoiId} has no Vietnamese localization — skipping", poiId);
            return;
        }

        foreach (var lang in DefaultLanguages)
        {
            if (poi.Localizations.Any(l => l.Language == lang)) continue;
            await TranslateAndGenerateAudioAsync(poi, viLocalization.TextContent, lang, cancellationToken);
        }

        // Always generate audio for Vietnamese too if missing
        if (viLocalization.AudioUrl is null)
            await GenerateAudioAsync(poi, viLocalization, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task TranslateAndGenerateAudioAsync(
        Poi poi, string sourceText, string targetLang, CancellationToken cancellationToken)
    {
        try
        {
            var translateClient = httpClientFactory.CreateClient("DeepTranslate");
            var translateResp = await translateClient.PostAsJsonAsync("/translate",
                new { text = sourceText, source_language = "vi", target_language = targetLang },
                cancellationToken);

            if (!translateResp.IsSuccessStatusCode) return;

            var result = await translateResp.Content.ReadFromJsonAsync<TranslateResult>(cancellationToken: cancellationToken);
            if (result?.translated_text is null) return;

            var localization = new PoiLocalization
            {
                PoiId = poi.Id,
                Language = targetLang,
                TextContent = result.translated_text,
            };
            db.PoiLocalizations.Add(localization);
            await db.SaveChangesAsync(cancellationToken);

            await GenerateAudioAsync(poi, localization, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to localize POI {PoiId} to {Lang}", poi.Id, targetLang);
        }
    }

    private async Task GenerateAudioAsync(Poi poi, PoiLocalization localization, CancellationToken cancellationToken)
    {
        try
        {
            var ttsClient = httpClientFactory.CreateClient("TTS");
            var ttsResp = await ttsClient.PostAsJsonAsync("/tts/generate",
                new { poi_id = poi.Id.ToString(), language = localization.Language, text = localization.TextContent },
                cancellationToken);

            if (!ttsResp.IsSuccessStatusCode) return;

            var ttsResult = await ttsResp.Content.ReadFromJsonAsync<TtsResult>(cancellationToken: cancellationToken);
            if (ttsResult?.audio_url is null) return;

            localization.AudioUrl = ttsResult.audio_url;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate audio for POI {PoiId} lang {Lang}", poi.Id, localization.Language);
        }
    }

    private record TranslateResult(string translated_text);
    private record TtsResult(string audio_url);
}
