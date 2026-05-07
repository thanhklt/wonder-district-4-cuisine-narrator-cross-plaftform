using SQLite;

namespace Mobile.Models;

[Table("CachedPois")]
public class CachedPoi
{
    [PrimaryKey]
    public int PoiID { get; set; }
    public string PoiName { get; set; } = string.Empty;
    public string DescriptionVi { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Radius { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public string CoverImageUrl { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public DateTime CachedAt { get; set; }
}
