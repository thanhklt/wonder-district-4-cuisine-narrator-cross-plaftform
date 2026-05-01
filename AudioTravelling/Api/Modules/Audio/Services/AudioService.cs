using Api.Infrastructure.Helpers;
using Api.Modules.Audio.DTOs;
using Api.Modules.Audio.Interfaces;
using Api.Modules.Localization.Interfaces;
using Api.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Api.Modules.Audio.Services;

public class AudioService : IAudioService
{
    private readonly AppDbContext _db;
    private readonly ITtsProvider _ttsProvider;
    private readonly AudioFileService _audioFileService;
    private readonly ILocalizationService _localizationService;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public AudioService(
        AppDbContext db,
        ITtsProvider ttsProvider,
        AudioFileService audioFileService,
        ILocalizationService localizationService)
    {
        _db = db;
        _ttsProvider = ttsProvider;
        _audioFileService = audioFileService;
        _localizationService = localizationService;
    }

    public async Task<AudioResponse> GetPoiAudioAsync(
        int poiId,
        string lang,
        CancellationToken cancellationToken = default)
    {
        lang = LanguageHelper.Normalize(lang);
        var lockKey = $"{poiId}_{lang}";
        var semaphore = _locks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var poi = await _db.Pois
                .FirstOrDefaultAsync(x => x.PoiId == poiId, cancellationToken);

            if (poi == null)
                throw new Exception("POI not found");

            var voiceCode = VoiceMapper.GetVoice(lang);
            var isDefaultLanguage = lang is "vi" or "en" or "zh" or "ja" or "ru";

            if (lang == "vi")
            {
                var viPath = $"/uploads/audio/pois/{poiId}/vi.mp3";

                if (_audioFileService.AudioExists(viPath))
                {
                    return new AudioResponse
                    {
                        PoiId = poiId,
                        LanguageCode = "vi",
                        AudioUrl = viPath,
                        VoiceCode = voiceCode,
                        IsGenerated = false
                    };
                }

                var viText = poi.DescriptionVi ?? string.Empty;
                var viBytes = await _ttsProvider.SynthesizeAsync(viText, "vi", voiceCode, cancellationToken);
                var savedViPath = await _audioFileService.SavePoiAudioAsync(poiId, "vi", viBytes, cancellationToken);

                return new AudioResponse
                {
                    PoiId = poiId,
                    LanguageCode = "vi",
                    AudioUrl = savedViPath,
                    VoiceCode = voiceCode,
                    IsGenerated = true
                };
            }

            var localization = await _db.PoiLocalizations
                .FirstOrDefaultAsync(
                    x => x.PoiId == poiId && x.LanguageCode == lang,
                    cancellationToken);

            if (localization != null &&
                !string.IsNullOrWhiteSpace(localization.AudioUrl) &&
                _audioFileService.AudioExists(localization.AudioUrl))
            {
                return new AudioResponse
                {
                    PoiId = poiId,
                    LanguageCode = lang,
                    AudioUrl = localization.AudioUrl!,
                    VoiceCode = voiceCode,
                    IsGenerated = false
                };
            }

            string textToRead;

            if (localization != null)
            {
                textToRead = localization.Description ?? string.Empty;
            }
            else
            {
                var localizationResult = await _localizationService.GetOrCreateLocalizationAsync(
                    poiId,
                    lang,
                    cancellationToken);

                textToRead = localizationResult.Description;
            }

            var audioBytes = await _ttsProvider.SynthesizeAsync(
                textToRead,
                lang,
                voiceCode,
                cancellationToken);

            var savedPath = await _audioFileService.SavePoiAudioAsync(
                poiId,
                lang,
                audioBytes,
                cancellationToken);

            if (isDefaultLanguage && localization != null)
            {
                localization.AudioUrl = savedPath;
                localization.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);
            }

            return new AudioResponse
            {
                PoiId = poiId,
                LanguageCode = lang,
                AudioUrl = savedPath,
                VoiceCode = voiceCode,
                IsGenerated = true
            };
        }
        finally
        {
            semaphore.Release();
        }
    }
}