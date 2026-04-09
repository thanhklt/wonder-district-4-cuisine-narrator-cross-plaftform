using Microsoft.EntityFrameworkCore;
using WebAdmin.Data;
using WebAdmin.Models;
using WebAdmin.Repositories.Interfaces;

namespace WebAdmin.Repositories.Implementations
{
    public class StoreRepository : IStoreRepository
    {
        private readonly AppDbContext _context;

        public StoreRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Store>> GetAllAsync()
        {
            return await _context.Stores
                .Include(s => s.Owner)
                .Include(s => s.Galleries)
                .Include(s => s.StoreItems)
                .ToListAsync();
        }

        public async Task<Store?> GetByIdAsync(int id)
        {
            return await _context.Stores
                .Include(s => s.Owner)
                .Include(s => s.Galleries)
                .Include(s => s.StoreItems)
                .FirstOrDefaultAsync(s => s.StoreID == id);
        }

        public async Task<Store> CreateAsync(Store store)
        {
            _context.Stores.Add(store);
            await _context.SaveChangesAsync();
            return store;
        }

        public async Task<Store> UpdateAsync(Store store)
        {
            _context.Entry(store).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return store;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var store = await _context.Stores.FindAsync(id);
            if (store == null) return false;

            _context.Stores.Remove(store);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}