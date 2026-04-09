using WebAdmin.Models;

namespace WebAdmin.Services.Interfaces
{
    public interface IOrderDetailService
    {
        Task<IEnumerable<OrderDetail>> GetAllAsync();
        Task<OrderDetail?> GetByIdAsync(int id);
    }
}