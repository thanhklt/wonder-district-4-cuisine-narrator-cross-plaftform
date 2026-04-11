using Api.Infrastructure.Services;
using Api.Modules.Auth.DTOs;
using Api.Modules.Auth.Interfaces;
using Api.Persistence;
using Api.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthService(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<SignupResponse> SignupAsync(SignupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new Exception("Email is required.");

            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new Exception("Full name is required.");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new Exception("Password is required.");

            if (request.Password != request.ConfirmPassword)
                throw new Exception("Password confirmation does not match.");

            var normalizedEmail = request.Email.Trim().ToLower();

            var existedUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail);

            if (existedUser != null)
                throw new Exception("Email already exists.");

            var userRole = await _context.Roles.FirstOrDefaultAsync(x => x.RoleName == "User");
            if (userRole == null)
                throw new Exception("Default role 'User' was not found.");

            var user = new User
            {
                Email = normalizedEmail,
                FullName = request.FullName.Trim(),
                RoleId = userRole.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new SignupResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                Message = "Signup successful."
            };
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new Exception("Email and password are required.");

            var normalizedEmail = request.Email.Trim().ToLower();

            var user = await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail);

            if (user == null)
                throw new Exception("Invalid email or password.");

            if (!user.IsActive)
                throw new Exception("This account has been disabled.");

            var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verifyResult == PasswordVerificationResult.Failed)
                throw new Exception("Invalid email or password.");
            //if (user.PasswordHash != request.Password)
            //    throw new Exception("Invalid email or password.");

            var roleName = user.Role.RoleName;
            var (token, expiresAtUtc) = _jwtService.GenerateToken(user, roleName);

            return new LoginResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Role = roleName,
                AccessToken = token,
                ExpiresAtUtc = expiresAtUtc
            };
        }

        public async Task<MeResponse?> GetMeAsync(int userId)
        {
            var user = await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (user == null)
                return null;

            return new MeResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.RoleName,
                IsActive = user.IsActive
            };
        }
    }
}