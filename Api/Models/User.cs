using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models.Entities
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
}
