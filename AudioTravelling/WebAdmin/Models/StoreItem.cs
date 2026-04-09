namespace WebAdmin.Models
{
    public class StoreItem
    {
        public int StoreItemID { get; set; }
        public string StoreItemName { get; set; }
        public int StoreItemStatus { get; set; }
        public decimal StoreItemPrice { get; set; }
        public string StoreItemImageLink { get; set; }
        public int? StoreItemDiscount { get; set; }
        public int StoreID { get; set; }

        public Store Store { get; set; }

        public ICollection<CartItem> CartItems { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}