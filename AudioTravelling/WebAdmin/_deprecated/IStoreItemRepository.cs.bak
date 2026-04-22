using WebAdmin.Models;

namespace WebAdmin.Repositories.Interfaces
{
    public interface IStoreItemRepository
    {
        Task<IEnumerable<StoreItem>> GetAllAsync();
        Task<StoreItem?> GetByIdAsync(int id);
        Task<StoreItem> CreateAsync(StoreItem storeItem);
        Task<StoreItem> UpdateAsync(StoreItem storeItem);
        Task<bool> DeleteAsync(int id);
    }
}