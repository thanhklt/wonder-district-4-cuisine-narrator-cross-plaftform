using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models.Entities
{
    [Table("Pois")]
    public class Poi
    {
        [Key]
        public int PoiID { get; set; }
        public string PoiName { get; set; } = string.Empty;
        public string DescriptionVi { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Status { get; set; } = "Pending";
        public bool IsActive { get; set; }
        public int PackageId { get; set; }
        public int OwnerID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        [ForeignKey("OwnerID")]
        public virtual User Owner { get; set; }
        [ForeignKey("PackageId")]
        public virtual Package Package { get; set; }
        public virtual ICollection<PoiImage> Images { get; set; } = new List<PoiImage>();
        public virtual ICollection<PoiLocalization> Localizations { get; set; } = new List<PoiLocalization>();
        public virtual ICollection<PoiApprovalLog> ApprovalLogs { get; set; } = new List<PoiApprovalLog>();
    }
}
