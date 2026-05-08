namespace Api.Models
{
    public class PackageDto
    {
        public int PackageId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Radius { get; set; }
        public int Priority { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
