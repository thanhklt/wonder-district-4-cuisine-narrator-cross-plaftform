using SQLite;

namespace AudioTravelling.Mobile.Data.SQLite.Entities;

[Table("AudioPlaybackHistory")]
public class AudioPlaybackHistory
{
    [PrimaryKey, AutoIncrement]
    public int PlaybackId { get; set; }

    public int PoiId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string TriggerSource { get; set; } = string.Empty;

    public string StartedAtUtc { get; set; } = string.Empty;

    public string EndedAtUtc { get; set; } = string.Empty;

    public int WasCompleted { get; set; }

    public int WasInterrupted { get; set; }
}