using WebAdmin.Models;

namespace WebAdmin.Services.Interfaces
{
    public interface ICartItemService
    {
        Task<IEnumerable<CartItem>> GetAllAsync();
        Task<CartItem?> GetByIdAsync(int id);
    }
}