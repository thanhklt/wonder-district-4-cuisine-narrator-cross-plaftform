using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models.Entities
{
    [Table("PoiApprovalLogs")]
    public class PoiApprovalLog
    {
        [Key]
        public int LogID { get; set; }
        public int PoiID { get; set; }
        public int PerformedBy { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        [ForeignKey("PoiID")]
        public virtual Poi Poi { get; set; }
    }
}
