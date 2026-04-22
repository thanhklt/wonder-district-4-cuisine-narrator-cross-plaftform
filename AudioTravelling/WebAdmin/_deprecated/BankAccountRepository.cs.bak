using Microsoft.EntityFrameworkCore;
using WebAdmin.Data;
using WebAdmin.Models;
using WebAdmin.Repositories.Interfaces;

namespace WebAdmin.Repositories.Implementations
{
    public class BankAccountRepository : IBankAccountRepository
    {
        private readonly AppDbContext _context;

        public BankAccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BankAccount>> GetAllAsync()
        {
            return await _context.BankAccounts.ToListAsync();
        }

        public async Task<BankAccount?> GetByIdAsync(string id)
        {
            return await _context.BankAccounts.FindAsync(id);
        }

        public async Task<BankAccount> CreateAsync(BankAccount bankAccount)
        {
            _context.BankAccounts.Add(bankAccount);
            await _context.SaveChangesAsync();
            return bankAccount;
        }

        public async Task<BankAccount> UpdateAsync(BankAccount bankAccount)
        {
            _context.Entry(bankAccount).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return bankAccount;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var bankAccount = await _context.BankAccounts.FindAsync(id);
            if (bankAccount == null) return false;

            _context.BankAccounts.Remove(bankAccount);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}