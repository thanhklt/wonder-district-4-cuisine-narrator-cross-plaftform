using SQLite;

namespace AudioTravelling.Mobile.Data.Models;

public class CachedPoiAudio
{
    [PrimaryKey, AutoIncrement]
    public int AudioCacheId { get; set; }

    public int PoiId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string AudioUrl { get; set; } = string.Empty;
    public string LocalFilePath { get; set; } = string.Empty;
    public string VoiceCode { get; set; } = string.Empty;
    public string DownloadStatus { get; set; } = string.Empty;
    public string CachedAtUtc { get; set; } = DateTime.UtcNow.ToString("O");
}