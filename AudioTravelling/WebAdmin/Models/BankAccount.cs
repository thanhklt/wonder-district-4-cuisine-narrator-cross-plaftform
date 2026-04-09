namespace WebAdmin.Models
{
    public class BankAccount
    {
        public string BankAccountID { get; set; }
        public decimal BankBalance { get; set; }
        public string BankName { get; set; }

        public ICollection<User> Users { get; set; }
    }
}