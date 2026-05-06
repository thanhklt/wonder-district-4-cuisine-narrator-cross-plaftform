namespace Api.Models
{
    public class QrDto
    {
        public int QrId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string TargetUrl { get; set; } = string.Empty;
        public string QrPayload { get; set; } = string.Empty;
        public string? QrImageUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ScanCount { get; set; }
    }

    public class CreateQrRequest
    {
        public DateTime? ExpiredAt { get; set; }
    }
}
