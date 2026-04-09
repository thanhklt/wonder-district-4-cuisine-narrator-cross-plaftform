using WebAdmin.Models;

namespace WebAdmin.Repositories.Interfaces
{
    public interface IStoreRepository
    {
        Task<IEnumerable<Store>> GetAllAsync();
        Task<Store?> GetByIdAsync(int id);
        Task<Store> CreateAsync(Store store);
        Task<Store> UpdateAsync(Store store);
        Task<bool> DeleteAsync(int id);
    }
}