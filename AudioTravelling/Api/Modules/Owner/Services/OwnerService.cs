namespace Api.Modules.Owner.Services;

using Api.Infrastructure.Services;
using Api.Modules.Auth.DTOs;
using Api.Modules.Owner.DTOs;
using Api.Modules.Owner.Interfaces;
using Api.Persistence;
using Api.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class OwnerService : IOwnerService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JwtService _jwtService;

    public OwnerService(
        AppDbContext context,
        IPasswordHasher<User> passwordHasher,
        JwtService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> SignupAsync(OwnerSignupRequest request)
    {
        // 1. Check email tồn tại
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (existingUser != null)
            throw new Exception("Email already exists");

        // 2. Lấy role Owner
        var ownerRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleName == "Owner");

        if (ownerRole == null)
            throw new Exception("Role Owner not found");

        // 3. Tạo user
        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName,
            RoleId = ownerRole.RoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // 4. Hash password
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // 5. Tạo JWT
        var (token, expiresAtUtc) = _jwtService.GenerateToken(user, ownerRole.RoleName);

        return new LoginResponse
        {
            AccessToken = token,
            UserId = user.UserId,
            Email = user.Email,
            Role = ownerRole.RoleName
        };
    }
}