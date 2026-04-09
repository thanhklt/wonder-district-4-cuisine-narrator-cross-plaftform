namespace WebAdmin.Models
{
    public class OrderDetail
    {
        public int OrderDetailID { get; set; }
        public int OrderDetailStoreItemID { get; set; }
        public int OrderDetailQuantity { get; set; }
        public int OrderID { get; set; }

        public Order Order { get; set; }
        public StoreItem StoreItem { get; set; }
    }
}