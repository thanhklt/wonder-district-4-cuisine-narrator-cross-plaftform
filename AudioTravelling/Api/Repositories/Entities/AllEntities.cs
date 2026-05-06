using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Repositories.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int UserID { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int RoleID { get; set; }
        public int UserStatus { get; set; }
        public DateTime CreatedDate { get; set; }

        [ForeignKey("RoleID")]
        public virtual Role Role { get; set; }
        public virtual ICollection<Poi> Pois { get; set; } = new List<Poi>();
    }

    [Table("Roles")]
    public class Role
    {
        [Key]
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }

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

    [Table("PoiImages")]
    public class PoiImage
    {
        [Key]
        public int ImageID { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsCover { get; set; }
        public int DisplayOrder { get; set; }
        public int PoiID { get; set; }

        [ForeignKey("PoiID")]
        public virtual Poi Poi { get; set; }
    }

    [Table("PoiLocalizations")]
    public class PoiLocalization
    {
        [Key]
        public int LocalizationID { get; set; }
        public int PoiID { get; set; }
        public string LanguageCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AudioUrl { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        [ForeignKey("PoiID")]
        public virtual Poi Poi { get; set; }
    }

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

    [Table("Packages")]
    public class Package
    {
        [Key]
        public int PackageId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Radius { get; set; }
        public int Priority { get; set; }
        public decimal Price { get; set; }
    }

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
