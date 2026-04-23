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
            var existing = poi.Localizations.FirstOrDefault(l => l.Language == lang);
            if (existing is null)
            {
                // Chưa có bản dịch: dịch text + generate TTS
                await TranslateAndGenerateAudioAsync(poi, viLocalization.TextContent, lang, cancellationToken);
            }
            else if (existing.AudioUrl is null)
            {
                // Đã có bản dịch (từ lúc tạo/cập nhật): chỉ generate TTS
                await GenerateAudioAsync(poi, existing, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        if (viLocalization.AudioUrl is null)
            await GenerateAudioAsync(poi, viLocalization, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task TranslateOnlyAsync(Guid poiId, CancellationToken cancellationToken = default)
    {
        var poi = await db.Pois
            .Include(p => p.Localizations)
            .FirstOrDefaultAsync(p => p.Id == poiId, cancellationToken);

        if (poi is null) return;

        var viLocalization = poi.Localizations.FirstOrDefault(l => l.Language == "vi");
        if (viLocalization is null) return;

        foreach (var lang in DefaultLanguages)
        {
            if (poi.Localizations.Any(l => l.Language == lang)) continue;
            await TranslateTextOnlyAsync(poi, viLocalization.TextContent, lang, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<(int success, int failed)> GenerateAudioBulkAsync(
        string[] languages, CancellationToken cancellationToken = default)
    {
        var locs = await db.PoiLocalizations
            .Include(l => l.Poi)
            .Where(l => languages.Contains(l.Language) && l.AudioUrl == null)
            .ToListAsync(cancellationToken);

        int success = 0, failed = 0;

        foreach (var loc in locs)
        {
            try
            {
                var ttsClient = httpClientFactory.CreateClient("TTS");
                var ttsResp = await ttsClient.PostAsJsonAsync("/tts/generate",
                    new { poi_id = loc.PoiId.ToString(), language = loc.Language, text = loc.TextContent },
                    cancellationToken);

                if (!ttsResp.IsSuccessStatusCode) { failed++; continue; }

                var result = await ttsResp.Content.ReadFromJsonAsync<TtsResult>(cancellationToken: cancellationToken);
                if (result?.audio_url is null) { failed++; continue; }

                loc.AudioUrl = result.audio_url;
                await db.SaveChangesAsync(cancellationToken);
                success++;
                logger.LogInformation("Generated audio: POI {PoiId} lang {Lang}", loc.PoiId, loc.Language);
            }
            catch (Exception ex)
            {
                failed++;
                logger.LogError(ex, "Failed audio: POI {PoiId} lang {Lang}", loc.PoiId, loc.Language);
            }
        }

        return (success, failed);
    }

    private async Task TranslateTextOnlyAsync(
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

            db.PoiLocalizations.Add(new PoiLocalization
            {
                PoiId = poi.Id,
                Language = targetLang,
                TextContent = result.translated_text,
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to translate POI {PoiId} to {Lang}", poi.Id, targetLang);
        }
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
