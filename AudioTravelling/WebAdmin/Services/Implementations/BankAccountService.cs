using WebAdmin.Models;
using WebAdmin.Repositories.Interfaces;
using WebAdmin.Services.Interfaces;

namespace WebAdmin.Services.Implementations
{
    public class BankAccountService : IBankAccountService
    {
        private readonly IBankAccountRepository _repository;

        public BankAccountService(IBankAccountRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<BankAccount>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<BankAccount?> GetByIdAsync(string id)
        {
            return await _repository.GetByIdAsync(id);
        }
    }
}