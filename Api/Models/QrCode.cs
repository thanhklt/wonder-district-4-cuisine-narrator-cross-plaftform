using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models.Entities
{
    [Table("QrCodes")]
    public class QrCode
    {
        [Key]
        public int QrCodeID { get; set; }
        [Column("QrCode")]
        public string QrCodeValue { get; set; } = string.Empty;
        public DateTime? ExpiredAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<AccessSession> Sessions { get; set; } = new List<AccessSession>();
    }
}
