using Api.Repositories;
using Api.Models;
using Api.Services;
using Api.DTOs;
using System.Diagnostics.Eventing.Reader;

namespace Api.Services
{
	public class AuthService
	{
		private UserRepository _repo;
		private JwtService _jwtService;

		public AuthService(UserRepository repo, JwtService jwtService)
		{
			this._repo = repo;
			this._jwtService = jwtService;
		}

		public async Task<SignupResponse> Signup(SignupRequest request)
		{
			var user = new User
			{
				Email = request.Email,
				PasswordHash = request.Password,
				PhoneNumber = request.PhoneNumber,
				FullName = request.FullName,
				RoleID = request.RoleId,
				UserStatus = 1,
				CreatedDate = DateTime.Now
			};

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
			var tmp_user = new User
			{
				Email = request.Email,
				PasswordHash = request.Password
			};

			var user = await _repo.LoginUser(tmp_user);
			if (user == null)
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