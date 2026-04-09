using WebAdmin.Models;
using WebAdmin.Repositories.Interfaces;
using WebAdmin.Services.Interfaces;

namespace WebAdmin.Services.Implementations
{
    public class StoreService : IStoreService
    {
        private readonly IStoreRepository _repository;

        public StoreService(IStoreRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Store>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Store?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
    }
}