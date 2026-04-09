namespace WebAdmin.Models
{
    public class Package
    {
        public int PackageID { get; set; }
        public string PackageName { get; set; }

        public ICollection<User> Users { get; set; }
    }
}