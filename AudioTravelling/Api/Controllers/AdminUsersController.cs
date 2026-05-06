using Api.Models;
using Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/admin/users")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminUsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? role, [FromQuery] int? status, [FromQuery] string? search)
        {
            var query = _context.Users.Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrEmpty(role))
                query = query.Where(u => u.Role.RoleName == role);

            if (status.HasValue)
                query = query.Where(u => u.UserStatus == status.Value);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search) || u.PhoneNumber.Contains(search));

            var users = await query.Select(u => new UserDto
            {
                UserId = u.UserID, FullName = u.FullName, Email = u.Email,
                PhoneNumber = u.PhoneNumber, RoleId = u.RoleID, Role = u.Role.RoleName,
                UserStatus = u.UserStatus, Status = u.UserStatus == 1 ? "Active" : "Inactive",
                CreatedDate = u.CreatedDate, IsLocked = u.UserStatus == 0, LastLoginAt = null
            }).ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserID == id);
            if (user == null) return NotFound();

            return Ok(new UserDto
            {
                UserId = user.UserID, FullName = user.FullName, Email = user.Email,
                PhoneNumber = user.PhoneNumber, RoleId = user.RoleID, Role = user.Role.RoleName,
                UserStatus = user.UserStatus, Status = user.UserStatus == 1 ? "Active" : "Inactive",
                CreatedDate = user.CreatedDate, IsLocked = user.UserStatus == 0, LastLoginAt = null
            });
        }
    }
}
