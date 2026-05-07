using Api.Models;
using Api.Models.Entities;
using Api.Repositories;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/admin/profiles")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminProfilesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminProfilesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? role, [FromQuery] string? search)
        {
            var query = _context.Users.Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(u => u.Role.RoleName.Contains(role));
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.FullName.Contains(search) || 
                                         u.Email.Contains(search) || 
                                         u.PhoneNumber.Contains(search));
            }

            var profiles = await query.Select(u => new ProfileDto
            {
                UserId = u.UserID,
                FullName = u.FullName,
                Email = u.Email,
                RoleId = u.RoleID,
                Role = u.Role.RoleName,
                PhoneNumber = u.PhoneNumber,
                Status = u.UserStatus == 1 ? "Active" : "Inactive",
                CreatedDate = u.CreatedDate,
                AvatarUrl = null,
                Address = null,
                UpdatedAt = null
            }).ToListAsync();

            return Ok(profiles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserID == id);
            if (user == null) return NotFound();

            return Ok(new ProfileDto
            {
                UserId = user.UserID,
                FullName = user.FullName,
                Email = user.Email,
                RoleId = user.RoleID,
                Role = user.Role.RoleName,
                PhoneNumber = user.PhoneNumber,
                Status = user.UserStatus == 1 ? "Active" : "Inactive",
                CreatedDate = user.CreatedDate,
                AvatarUrl = null,
                Address = null,
                UpdatedAt = null
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProfileRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.FullName = request.FullName ?? user.FullName;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated successfully" });
        }
    }

    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
