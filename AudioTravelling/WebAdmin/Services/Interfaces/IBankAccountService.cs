using WebAdmin.Models;

namespace WebAdmin.Services.Interfaces
{
    public interface IBankAccountService
    {
        Task<IEnumerable<BankAccount>> GetAllAsync();
        Task<BankAccount?> GetByIdAsync(string id);
    }
}