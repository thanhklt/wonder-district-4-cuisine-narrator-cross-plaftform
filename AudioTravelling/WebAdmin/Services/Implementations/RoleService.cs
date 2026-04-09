using WebAdmin.Models;
using WebAdmin.Repositories.Interfaces;
using WebAdmin.Services.Interfaces;

namespace WebAdmin.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _repository;

        public RoleService(IRoleRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Role?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
    }
}