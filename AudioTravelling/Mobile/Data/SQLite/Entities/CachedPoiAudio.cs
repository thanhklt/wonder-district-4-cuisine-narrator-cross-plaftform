using SQLite;

namespace AudioTravelling.Mobile.Data.SQLite.Entities;

[Table("CachedPoiAudios")]
public class CachedPoiAudio
{
    [PrimaryKey]
    public int AudioCacheId { get; set; }

    public int PoiId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string AudioUrl { get; set; } = string.Empty;

    public string LocalFilePath { get; set; } = string.Empty;

    public string VoiceCode { get; set; } = string.Empty;

    public int DurationSeconds { get; set; }

    public string DownloadStatus { get; set; } = "pending"; // pending / downloaded / failed

    public string LastPlayedAtUtc { get; set; } = string.Empty;

    public string CachedAtUtc { get; set; } = string.Empty;
}