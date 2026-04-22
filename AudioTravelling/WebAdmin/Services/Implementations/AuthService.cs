using WebAdmin.Services.Interfaces;

namespace WebAdmin.Services.Implementations
{
    /// <summary>
    /// Mock authentication service with hardcoded users.
    /// Replace with real DB authentication later.
    /// </summary>
    public class AuthService : IAuthService
    {
        // ── Mock user accounts ──
        private static readonly List<MockUser> _mockUsers = new()
        {
            new MockUser
            {
                UserId = 1,
                Email = "admin@audiotravelling.vn",
                Password = "admin123",
                FullName = "Nguyễn Văn Admin",
                Role = "Admin",
                AvatarUrl = "https://i.pravatar.cc/150?img=11"
            },
            new MockUser
            {
                UserId = 2,
                Email = "owner@audiotravelling.vn",
                Password = "owner123",
                FullName = "Trần Thị Owner",
                Role = "Owner",
                AvatarUrl = "https://i.pravatar.cc/150?img=5"
            },
            new MockUser
            {
                UserId = 3,
                Email = "owner2@audiotravelling.vn",
                Password = "owner123",
                FullName = "Lê Minh Quán",
                Role = "Owner",
                AvatarUrl = "https://i.pravatar.cc/150?img=8"
            }
        };

        public Task<AuthResult?> AuthenticateAsync(string email, string password)
        {
            var user = _mockUsers.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);

            if (user == null)
                return Task.FromResult<AuthResult?>(null);

            var result = new AuthResult
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                AvatarUrl = user.AvatarUrl
            };

            return Task.FromResult<AuthResult?>(result);
        }

        /// <summary>
        /// Internal mock user class
        /// </summary>
        private class MockUser
        {
            public int UserId { get; set; }
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public string? AvatarUrl { get; set; }
        }
    }
}
