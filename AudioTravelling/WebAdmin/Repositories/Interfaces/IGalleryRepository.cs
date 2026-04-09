using WebAdmin.Models;

namespace WebAdmin.Repositories.Interfaces
{
    public interface IGalleryRepository
    {
        Task<IEnumerable<Gallery>> GetAllAsync();
        Task<Gallery?> GetByIdAsync(int id);
        Task<Gallery> CreateAsync(Gallery gallery);
        Task<Gallery> UpdateAsync(Gallery gallery);
        Task<bool> DeleteAsync(int id);
    }
}