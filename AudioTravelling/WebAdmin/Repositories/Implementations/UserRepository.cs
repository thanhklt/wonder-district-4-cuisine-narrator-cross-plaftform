using Microsoft.EntityFrameworkCore;
using WebAdmin.Data;
using WebAdmin.Models;
using WebAdmin.Repositories.Interfaces;

namespace WebAdmin.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Package)
                .Include(u => u.BankAccount)
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Package)
                .Include(u => u.BankAccount)
                .FirstOrDefaultAsync(u => u.UserID == id);
        }

        public async Task<User> CreateAsync(User user)
        {
            user.CreatedDate = DateTime.Now;
            user.UpdatedDate = DateTime.Now;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            user.UpdatedDate = DateTime.Now;

            _context.Entry(user).State = EntityState.Modified;
            _context.Entry(user).Property(u => u.CreatedDate).IsModified = false;

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}