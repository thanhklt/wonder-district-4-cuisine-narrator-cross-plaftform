using WebAdmin.Models;

namespace WebAdmin.Repositories.Interfaces
{
    public interface IBankAccountRepository
    {
        Task<IEnumerable<BankAccount>> GetAllAsync();
        Task<BankAccount?> GetByIdAsync(string id);
        Task<BankAccount> CreateAsync(BankAccount bankAccount);
        Task<BankAccount> UpdateAsync(BankAccount bankAccount);
        Task<bool> DeleteAsync(string id);
    }
}