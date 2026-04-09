using WebAdmin.Models;

namespace WebAdmin.Services.Interfaces
{
    public interface IStoreService
    {
        Task<IEnumerable<Store>> GetAllAsync();
        Task<Store?> GetByIdAsync(int id);
    }
}