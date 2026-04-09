namespace WebAdmin.Models
{
    public class Store
    {
        public int StoreID { get; set; }
        public string StoreName { get; set; }
        public decimal StoreLatitude { get; set; }
        public decimal StoreLongitude { get; set; }
        public decimal StoreRadius { get; set; }
        public string? StoreSummary { get; set; }
        public int StoreStatus { get; set; }
        public int OwnerID { get; set; }

        public User Owner { get; set; }

        public ICollection<Gallery> Galleries { get; set; }
        public ICollection<StoreItem> StoreItems { get; set; }
    }
}