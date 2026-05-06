namespace Api.Models
{
    public class PoiDto
    {
        public int PoiId { get; set; }
        public string PoiName { get; set; } = string.Empty;
        public string DescriptionVi { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Radius { get; set; }
        public int Priority { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int PackageId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public int OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string? ImageUrl { get; set; }
        public List<string> Images { get; set; } = new List<string>();
    }

    public class CreatePoiRequest
    {
        public string PoiName { get; set; } = string.Empty;
        public string DescriptionVi { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int PackageId { get; set; }
        public IFormFile? ImageFile { get; set; }
    }

    public class UpdatePoiRequest : CreatePoiRequest
    {
    }

    public class RejectPoiRequest
    {
        public string Note { get; set; } = string.Empty;
    }
}
