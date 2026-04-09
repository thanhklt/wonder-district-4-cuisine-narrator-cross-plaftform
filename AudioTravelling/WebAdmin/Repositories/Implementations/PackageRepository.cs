using Microsoft.EntityFrameworkCore;
using WebAdmin.Data;
using WebAdmin.Models;
using WebAdmin.Repositories.Interfaces;

namespace WebAdmin.Repositories.Implementations
{
    public class PackageRepository : IPackageRepository
    {
        private readonly AppDbContext _context;

        public PackageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Package>> GetAllAsync()
        {
            return await _context.Packages.ToListAsync();
        }

        public async Task<Package?> GetByIdAsync(int id)
        {
            return await _context.Packages.FindAsync(id);
        }

        public async Task<Package> CreateAsync(Package package)
        {
            _context.Packages.Add(package);
            await _context.SaveChangesAsync();
            return package;
        }

        public async Task<Package> UpdateAsync(Package package)
        {
            _context.Entry(package).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return package;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var package = await _context.Packages.FindAsync(id);
            if (package == null) return false;

            _context.Packages.Remove(package);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}