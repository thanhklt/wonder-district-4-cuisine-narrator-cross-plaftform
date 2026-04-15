namespace AudioTravelling.Mobile.Services.Api.Responses;

public class SyncBootstrapResponse
{
    public string ServerTimeUtc { get; set; } = string.Empty;
    public List<SyncPoiDto> Pois { get; set; } = new();
}

public class SyncPoiDto
{
    public int PoiId { get; set; }
    public string NameDefault { get; set; } = string.Empty;
    public string DescriptionDefault { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int RadiusMeters { get; set; }
    public int Priority { get; set; }
    public string CoverImageUrl { get; set; } = string.Empty;
    public int PackageId { get; set; }
    public bool IsActive { get; set; }
    public string UpdatedAtUtc { get; set; } = string.Empty;

    public List<SyncPoiImageDto> Images { get; set; } = new();
    public List<SyncPoiLocalizationDto> Localizations { get; set; } = new();
    public List<SyncPoiAudioDto> Audios { get; set; } = new();
}

public class SyncPoiImageDto
{
    public int ImageId { get; set; }
    public int PoiId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsCover { get; set; }
    public string UpdatedAtUtc { get; set; } = string.Empty;
}

public class SyncPoiLocalizationDto
{
    public int LocalizationId { get; set; }
    public int PoiId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TranslationSource { get; set; } = string.Empty;
    public string UpdatedAtUtc { get; set; } = string.Empty;
}

public class SyncPoiAudioDto
{
    public int AudioCacheId { get; set; }
    public int PoiId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string AudioUrl { get; set; } = string.Empty;
    public string VoiceCode { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public string DownloadStatus { get; set; } = "pending";
}