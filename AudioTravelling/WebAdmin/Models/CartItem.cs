namespace WebAdmin.Models
{
    public class CartItem
    {
        public int CartItemID { get; set; }
        public int UserID { get; set; }
        public int StoreItemID { get; set; }
        public int Quantity { get; set; }

        public User User { get; set; }
        public StoreItem StoreItem { get; set; }
    }
}