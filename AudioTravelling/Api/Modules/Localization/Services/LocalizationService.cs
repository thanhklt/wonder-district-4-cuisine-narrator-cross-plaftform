using Api.Modules.Localization.DTOs;
using Api.Modules.Localization.Interfaces;
using Api.Persistence;
using Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Localization.Services;

public class LocalizationService : ILocalizationService
{
    private readonly AppDbContext _context;
    private readonly IDeepTranslateClient _deepTranslateClient;
    private readonly ILogger<LocalizationService> _logger;

    public LocalizationService(
        AppDbContext context,
        IDeepTranslateClient deepTranslateClient,
        ILogger<LocalizationService> logger)
    {
        _context = context;
        _deepTranslateClient = deepTranslateClient;
        _logger = logger;
    }

    public async Task<PoiLocalizationResponse> GetOrCreateLocalizationAsync(
        int poiId,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        if (poiId <= 0)
            throw new ArgumentException("Invalid poiId.");

        if (string.IsNullOrWhiteSpace(languageCode))
            throw new ArgumentException("Language code is required.");

        languageCode = NormalizeLanguageCode(languageCode);

        var existing = await _context.PoiLocalizations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.PoiId == poiId && x.LanguageCode == languageCode,
                cancellationToken);

        if (existing != null)
        {
            return new PoiLocalizationResponse
            {
                PoiId = existing.PoiId,
                LanguageCode = existing.LanguageCode,
                Name = existing.Name,
                Description = existing.Description
            };
        }

        var poi = await _context.Pois
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PoiId == poiId, cancellationToken);

        if (poi == null)
            throw new Exception("POI not found.");

        string localizedName;
        string localizedDescription;

        if (languageCode.Equals("vi", StringComparison.OrdinalIgnoreCase))
        {
            localizedName = poi.NameVi ?? string.Empty;
            localizedDescription = poi.DescriptionVi ?? string.Empty;
        }
        else
        {
            localizedName = string.IsNullOrWhiteSpace(poi.NameVi)
                ? string.Empty
                : await _deepTranslateClient.TranslateAsync(
                    poi.NameVi,
                    "vi",
                    languageCode,
                    cancellationToken);

            localizedDescription = string.IsNullOrWhiteSpace(poi.DescriptionVi)
                ? string.Empty
                : await _deepTranslateClient.TranslateAsync(
                    poi.DescriptionVi,
                    "vi",
                    languageCode,
                    cancellationToken);
        }

        var now = DateTime.UtcNow;

        var localization = new PoiLocalization
        {
            PoiId = poiId,
            LanguageCode = languageCode,
            Name = localizedName,
            Description = localizedDescription,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.PoiLocalizations.Add(localization);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "Failed to save localization for PoiId={PoiId}, LanguageCode={LanguageCode}",
                poiId,
                languageCode);

            throw new Exception("Failed to save localization.", ex);
        }

        return new PoiLocalizationResponse
        {
            PoiId = localization.PoiId,
            LanguageCode = localization.LanguageCode,
            Name = localization.Name,
            Description = localization.Description
        };
    }

    private static string NormalizeLanguageCode(string languageCode)
    {
        languageCode = languageCode.Trim();

        return languageCode.ToLowerInvariant() switch
        {
            "vi-vn" => "vi",
            "en-us" => "en",
            "en-gb" => "en",
            "ja-jp" => "ja",
            "ru-ru" => "ru",
            "zh" => "zh-CN",
            "zh-cn" => "zh-CN",
            "zh-hans" => "zh-CN",
            _ => languageCode
        };
    }
}