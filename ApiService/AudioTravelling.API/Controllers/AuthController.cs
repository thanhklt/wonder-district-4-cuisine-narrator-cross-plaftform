using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AudioTravelling.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AudioTravelling.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAppDbContext db, IConfiguration config) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == req.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials" });

        var token = GenerateJwt(user.Id.ToString(), user.Email, user.Role.Name);
        return Ok(new { token, role = user.Role.Name, userId = user.Id });
    }

    private string GenerateJwt(string userId, string email, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT_SECRET"]!));
        var expiry = int.Parse(config["JWT_EXPIRY_HOURS"] ?? "24");
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
        };
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiry),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public record LoginRequest(string Email, string Password);
}
