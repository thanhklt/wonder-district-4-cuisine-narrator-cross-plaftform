using SQLite;

namespace Shared.Models
{
    public class LocalAudio
    {
        [PrimaryKey]
        public required string LanguageCode { get; set; } // Ví dụ: "en-US", "vi-VN"

        public required string FilePath { get; set; }     // Đường dẫn: /data/user/0/.../audio_en.mp3

        public DateTime DownloadedAt { get; set; } // Ngày tải để quản lý bộ nhớ

        public bool IsDownloaded { get; set; }   // Đánh dấu đã có sẵn hay chưa
    }
}
