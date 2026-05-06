using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models.Entities
{
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
}
