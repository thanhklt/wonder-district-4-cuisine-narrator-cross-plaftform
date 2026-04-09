using WebAdmin.Models;

namespace WebAdmin.Repositories.Interfaces
{
    public interface IPackageRepository
    {
        Task<IEnumerable<Package>> GetAllAsync();
        Task<Package?> GetByIdAsync(int id);
        Task<Package> CreateAsync(Package package);
        Task<Package> UpdateAsync(Package package);
        Task<bool> DeleteAsync(int id);
    }
}