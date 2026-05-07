using SQLite;

namespace Mobile.Models;

[Table("AudioPlaybackHistory")]
public class AudioPlaybackHistory
{
    [PrimaryKey, AutoIncrement]
    public int PlaybackID { get; set; }
    public int PoiID { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string TriggerSource { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public bool WasCompleted { get; set; }
    public bool WasInterrupted { get; set; }
}
