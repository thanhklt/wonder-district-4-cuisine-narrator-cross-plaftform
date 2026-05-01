namespace Api.Models
{
    public class Poi
    {
        public int PoiID { get; set; }
        public int OwnerID { get; set; }

        public string NameVi { get; set; } = string.Empty; // Tránh warning str null
        public string DescriptionVi { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public int Radius { get; set; }
        public int Priority { get; set; }
        public int PackageID { get; set; }

        public int ApprovalStatus { get; set; } // 0,1,2
        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}