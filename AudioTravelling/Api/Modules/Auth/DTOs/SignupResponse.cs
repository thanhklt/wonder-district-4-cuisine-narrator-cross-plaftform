namespace Api.Modules.Auth.DTOs
{
    public class SignupResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}