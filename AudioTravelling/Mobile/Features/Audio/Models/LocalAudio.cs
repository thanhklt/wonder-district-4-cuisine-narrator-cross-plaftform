using SQLite;

namespace AudioTravelling.Mobile.Features.Audio.Models;

public class LocalAudio
{
    [PrimaryKey]
    public string LanguageCode { get; set; } = string.Empty; // Ví dụ: "en-US", "vi-VN"

    public string FilePath { get; set; } = string.Empty;     // Đường dẫn: /data/user/0/.../audio_en.mp3

    public DateTime DownloadedAt { get; set; } // Ngày tải để quản lý bộ nhớ

    public bool IsDownloaded { get; set; }   // Đánh dấu đã có sẵn hay chưa
}
