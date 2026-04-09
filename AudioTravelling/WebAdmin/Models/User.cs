namespace WebAdmin.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? PackageID { get; set; }
        public string? BankAccountID { get; set; }
        public DateTime? SubscriptionDate { get; set; }
        public int UserStatus { get; set; }
        public int RoleID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public Role Role { get; set; }
        public Package? Package { get; set; }
        public BankAccount? BankAccount { get; set; }

        public ICollection<CartItem> CartItems { get; set; }
        public ICollection<Store> Stores { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}