using WebAdmin.Models;

namespace WebAdmin.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(int id);
    }
}