using Microsoft.EntityFrameworkCore;
using WebAdmin.Data;
using WebAdmin.Models;
using WebAdmin.Repositories.Interfaces;

namespace WebAdmin.Repositories.Implementations
{
    public class CartItemRepository : ICartItemRepository
    {
        private readonly AppDbContext _context;

        public CartItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CartItem>> GetAllAsync()
        {
            return await _context.CartItems
                .Include(ci => ci.User)
                .Include(ci => ci.StoreItem)
                .ToListAsync();
        }

        public async Task<CartItem?> GetByIdAsync(int id)
        {
            return await _context.CartItems
                .Include(ci => ci.User)
                .Include(ci => ci.StoreItem)
                .FirstOrDefaultAsync(ci => ci.CartItemID == id);
        }

        public async Task<CartItem> CreateAsync(CartItem cartItem)
        {
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();
            return cartItem;
        }

        public async Task<CartItem> UpdateAsync(CartItem cartItem)
        {
            _context.Entry(cartItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return cartItem;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null) return false;

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}