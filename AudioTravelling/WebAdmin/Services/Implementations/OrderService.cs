using WebAdmin.Models;
using WebAdmin.Repositories.Interfaces;
using WebAdmin.Services.Interfaces;

namespace WebAdmin.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;

        public OrderService(IOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
    }
}