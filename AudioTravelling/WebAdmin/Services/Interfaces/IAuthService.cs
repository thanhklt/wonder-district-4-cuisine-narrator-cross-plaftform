namespace WebAdmin.Services.Interfaces
{
    /// <summary>
    /// Authentication service for validating user credentials
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticate user by email and password.
        /// Returns (userId, fullName, role) if valid, null if invalid.
        /// </summary>
        Task<AuthResult?> AuthenticateAsync(string email, string password);
    }

    /// <summary>
    /// Result from a successful authentication
    /// </summary>
    public class AuthResult
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }
}
