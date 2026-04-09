namespace WebAdmin.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderPaymentMethod { get; set; }
        public decimal OrderRadius { get; set; }
        public string OrderStatus { get; set; }
        public string? OrderDescription { get; set; }
        public int OrderCustomerID { get; set; }

        public User Customer { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}