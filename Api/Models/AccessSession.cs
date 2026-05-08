using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models.Entities
{
    [Table("AccessSessions")]
    public class AccessSession
    {
        [Key]
        public int SessionID { get; set; }
        public int QrCodeID { get; set; }
        public string DeviceID { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiredAt { get; set; }
        public bool IsRevoked { get; set; }

        [ForeignKey("QrCodeID")]
        public virtual QrCode QrCode { get; set; }
    }
}
