namespace Api.Models
{
    public class Package
    {
        public int PackageID { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Radius { get; set; }
        public int Priority { get; set; }
        public float Price { get; set; }
    }
}