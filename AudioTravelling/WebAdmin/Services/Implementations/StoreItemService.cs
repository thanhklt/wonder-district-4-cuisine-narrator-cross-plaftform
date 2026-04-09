using WebAdmin.Models;
using WebAdmin.Repositories.Interfaces;
using WebAdmin.Services.Interfaces;

namespace WebAdmin.Services.Implementations
{
    public class StoreItemService : IStoreItemService
    {
        private readonly IStoreItemRepository _repository;

        public StoreItemService(IStoreItemRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<StoreItem>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<StoreItem?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
    }
}