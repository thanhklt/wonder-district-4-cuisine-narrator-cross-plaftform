using Microsoft.EntityFrameworkCore;
using WebAdmin.Data;
using WebAdmin.Models;
using WebAdmin.Repositories.Interfaces;

namespace WebAdmin.Repositories.Implementations
{
    public class GalleryRepository : IGalleryRepository
    {
        private readonly AppDbContext _context;

        public GalleryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Gallery>> GetAllAsync()
        {
            return await _context.Galleries
                .Include(g => g.Store)
                .ToListAsync();
        }

        public async Task<Gallery?> GetByIdAsync(int id)
        {
            return await _context.Galleries
                .Include(g => g.Store)
                .FirstOrDefaultAsync(g => g.GalleryID == id);
        }

        public async Task<Gallery> CreateAsync(Gallery gallery)
        {
            _context.Galleries.Add(gallery);
            await _context.SaveChangesAsync();
            return gallery;
        }

        public async Task<Gallery> UpdateAsync(Gallery gallery)
        {
            _context.Entry(gallery).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return gallery;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var gallery = await _context.Galleries.FindAsync(id);
            if (gallery == null) return false;

            _context.Galleries.Remove(gallery);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}