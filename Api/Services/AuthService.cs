using Api.Repositories;
using Api.Models.Entities;
using Api.DTOs;

using Microsoft.AspNetCore.Identity;

namespace Api.Services
{
    public class AuthService
    {
        private UserRepository _repo;
        private JwtService _jwtService;
        private IPasswordHasher<User> _passwordHasher;

        public AuthService(UserRepository repo, JwtService jwtService, IPasswordHasher<User> passwordHasher)
        {
            this._repo = repo;
            this._jwtService = jwtService;
            this._passwordHasher = passwordHasher;
        }

        public async Task<SignupResponse> Signup(SignupRequest request)
        {
            var user = new User
            {
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                FullName = request.FullName,
                RoleID = request.RoleId,
                UserStatus = 1,
                CreatedDate = DateTime.Now
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            var userId = await _repo.CreateUser(user);
            var res = new SignupResponse
            {
                UserId = userId,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                RoleId = user.RoleID,
                UserStatus = 1
            };
            return res;
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            var user = await _repo.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                throw new Exception("Email hoặc mật khẩu không đúng");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                throw new Exception("Email hoặc mật khẩu không đúng");
            }

            if (user.UserStatus != 1)
            {
                throw new Exception("Tài khoản đã bị khóa hoặc chưa kích hoạt");
            }

            var token = _jwtService.GenerateToken(user);

            // Tạo DTO response
            string RoleName = "";
            if (user.RoleID == 1)
                RoleName = "Admin";
            else if (user.RoleID == 2)
                RoleName = "Owner";

            var res = new LoginResponse
            {
                UserId = user.UserID,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                RoleName = RoleName,
                Token = token
            };
            return res;
        }

    }
}