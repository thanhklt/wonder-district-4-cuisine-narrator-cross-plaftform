namespace Api.Models
{
    public class RecentSessionDto
    {
        public int SessionId { get; set; }
        public int QrCodeId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiredAt { get; set; }
        public bool IsRevoked { get; set; }
    }
}
