using WebAdmin.Models;

namespace WebAdmin.Repositories.Interfaces
{
    public interface IOrderDetailRepository
    {
        Task<IEnumerable<OrderDetail>> GetAllAsync();
        Task<OrderDetail?> GetByIdAsync(int id);
        Task<OrderDetail> CreateAsync(OrderDetail orderDetail);
        Task<OrderDetail> UpdateAsync(OrderDetail orderDetail);
        Task<bool> DeleteAsync(int id);
    }
}