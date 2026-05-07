using SQLite;

namespace Mobile.Models;

[Table("CachedPoiAudios")]
public class CachedPoiAudio
{
    [PrimaryKey, AutoIncrement]
    public int AudioCacheID { get; set; }
    public int PoiID { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string AudioUrl { get; set; } = string.Empty;
    public string LocalFilePath { get; set; } = string.Empty;
    public string DownloadStatus { get; set; } = "pending";
    public int DurationSeconds { get; set; }
    public DateTime? LastPlayedAt { get; set; }
    public DateTime CachedAt { get; set; }
}
