using WebAdmin.Models;
using WebAdmin.Repositories.Interfaces;
using WebAdmin.Services.Interfaces;

namespace WebAdmin.Services.Implementations
{
    public class PackageService : IPackageService
    {
        private readonly IPackageRepository _repository;

        public PackageService(IPackageRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Package>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Package?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
    }
}