using WebAdmin.Models;
using WebAdmin.Repositories.Interfaces;
using WebAdmin.Services.Interfaces;

namespace WebAdmin.Services.Implementations
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IOrderDetailRepository _repository;

        public OrderDetailService(IOrderDetailRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<OrderDetail>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<OrderDetail?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
    }
}