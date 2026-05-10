using Api.Models;
using Api.Models.Entities;
using Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Identity;

namespace Api.Controllers
{
    [Route("api/admin/users")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AdminUsersController(AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
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
                StatusText = u.UserStatus == 1 ? "Đang hoạt động" : "Tạm dừng",
                CreatedDate = u.CreatedDate, IsLocked = u.UserStatus == 0, IsActive = u.UserStatus == 1, LastLoginAt = null
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
                StatusText = user.UserStatus == 1 ? "Đang hoạt động" : "Bị khóa",
                CreatedDate = user.CreatedDate, IsLocked = user.UserStatus == 0, IsActive = user.UserStatus == 1, LastLoginAt = null
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email đã tồn tại.");

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == request.Role);
            if (role == null) return BadRequest("Vai trò không hợp lệ.");

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber ?? "",
                RoleID = role.RoleID,
                UserStatus = 1,
                CreatedDate = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Created successfully", userId = user.UserID });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.UserID != id))
                return BadRequest("Email đã tồn tại.");

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == request.Role);
            if (role == null) return BadRequest("Vai trò không hợp lệ.");

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber ?? "";
            user.RoleID = role.RoleID;
            
            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Updated successfully" });
        }

        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.UserStatus = user.UserStatus == 1 ? 0 : 1;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Toggled successfully", isActive = user.UserStatus == 1 });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Không tìm thấy người dùng.");

            user.UserStatus = 0;
            await _context.SaveChangesAsync();
            return Ok(new { message = "User đã được xóa mềm." });
        }
    }

    public class CreateUserRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class UpdateUserRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
