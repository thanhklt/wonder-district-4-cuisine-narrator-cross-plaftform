using AudioTravelling.Mobile.Data.SQLite.Entities;
using AudioTravelling.Mobile.Services.Api.Responses;

namespace AudioTravelling.Mobile.Core.Sync;

public static class SyncMapper
{
    public static List<CachedPoi> ToCachedPois(IEnumerable<SyncPoiDto> pois, string cachedAtUtc)
    {
        return pois.Select(p => new CachedPoi
        {
            PoiId = p.PoiId,
            NameDefault = p.NameDefault,
            DescriptionDefault = p.DescriptionDefault,
            Latitude = p.Latitude,
            Longitude = p.Longitude,
            RadiusMeters = p.RadiusMeters,
            Priority = p.Priority,
            CoverImageUrl = p.CoverImageUrl,
            PackageId = p.PackageId,
            IsActive = p.IsActive ? 1 : 0,
            ServerUpdatedAtUtc = p.UpdatedAtUtc,
            CachedAtUtc = cachedAtUtc
        }).ToList();
    }

    public static List<CachedPoiImage> ToCachedPoiImages(IEnumerable<SyncPoiDto> pois, string cachedAtUtc)
    {
        return pois
            .SelectMany(p => p.Images)
            .Select(i => new CachedPoiImage
            {
                ImageId = i.ImageId,
                PoiId = i.PoiId,
                ImageUrl = i.ImageUrl,
                LocalFilePath = string.Empty,
                DisplayOrder = i.DisplayOrder,
                IsCover = i.IsCover ? 1 : 0,
                ServerUpdatedAtUtc = i.UpdatedAtUtc,
                CachedAtUtc = cachedAtUtc
            })
            .ToList();
    }

    public static List<CachedPoiLocalization> ToCachedPoiLocalizations(IEnumerable<SyncPoiDto> pois, string cachedAtUtc)
    {
        return pois
            .SelectMany(p => p.Localizations)
            .Select(l => new CachedPoiLocalization
            {
                LocalizationId = l.LocalizationId,
                PoiId = l.PoiId,
                LanguageCode = l.LanguageCode,
                Name = l.Name,
                Description = l.Description,
                TranslationSource = l.TranslationSource,
                ServerUpdatedAtUtc = l.UpdatedAtUtc,
                CachedAtUtc = cachedAtUtc
            })
            .ToList();
    }

    public static List<CachedPoiAudio> ToCachedPoiAudios(IEnumerable<SyncPoiDto> pois, string cachedAtUtc)
    {
        return pois
            .SelectMany(p => p.Audios)
            .Select(a => new CachedPoiAudio
            {
                AudioCacheId = a.AudioCacheId,
                PoiId = a.PoiId,
                LanguageCode = a.LanguageCode,
                AudioUrl = a.AudioUrl,
                LocalFilePath = string.Empty,
                VoiceCode = a.VoiceCode,
                DurationSeconds = a.DurationSeconds,
                DownloadStatus = string.IsNullOrWhiteSpace(a.DownloadStatus) ? "pending" : a.DownloadStatus,
                LastPlayedAtUtc = string.Empty,
                CachedAtUtc = cachedAtUtc
            })
            .ToList();
    }
}