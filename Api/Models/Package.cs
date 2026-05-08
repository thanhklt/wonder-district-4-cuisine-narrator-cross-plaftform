using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models.Entities
{
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
}
