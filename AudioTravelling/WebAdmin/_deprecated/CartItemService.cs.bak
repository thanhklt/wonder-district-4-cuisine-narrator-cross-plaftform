using WebAdmin.Models;
using WebAdmin.Repositories.Interfaces;
using WebAdmin.Services.Interfaces;

namespace WebAPI.Services.Implementations
{
    public class CartItemService : ICartItemService
    {
        private readonly ICartItemRepository _repository;

        public CartItemService(ICartItemRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CartItem>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<CartItem?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
    }
}