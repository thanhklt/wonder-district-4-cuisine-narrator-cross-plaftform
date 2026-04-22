using WebAdmin.Models;
using WebAdmin.Repositories.Interfaces;
using WebAdmin.Services.Interfaces;

namespace WebAdmin.Services.Implementations
{
    public class GalleryService : IGalleryService
    {
        private readonly IGalleryRepository _repository;

        public GalleryService(IGalleryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Gallery>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Gallery?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
    }
}