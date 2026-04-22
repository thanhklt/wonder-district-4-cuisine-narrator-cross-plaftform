using Microsoft.EntityFrameworkCore;
using WebAdmin.Data;
using WebAdmin.Models;
using WebAdmin.Repositories.Interfaces;

namespace WebAdmin.Repositories.Implementations
{
    public class StoreItemRepository : IStoreItemRepository
    {
        private readonly AppDbContext _context;

        public StoreItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StoreItem>> GetAllAsync()
        {
            return await _context.StoreItems
                .Include(si => si.Store)
                .ToListAsync();
        }

        public async Task<StoreItem?> GetByIdAsync(int id)
        {
            return await _context.StoreItems
                .Include(si => si.Store)
                .FirstOrDefaultAsync(si => si.StoreItemID == id);
        }

        public async Task<StoreItem> CreateAsync(StoreItem storeItem)
        {
            _context.StoreItems.Add(storeItem);
            await _context.SaveChangesAsync();
            return storeItem;
        }

        public async Task<StoreItem> UpdateAsync(StoreItem storeItem)
        {
            _context.Entry(storeItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return storeItem;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var storeItem = await _context.StoreItems.FindAsync(id);
            if (storeItem == null) return false;

            _context.StoreItems.Remove(storeItem);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}