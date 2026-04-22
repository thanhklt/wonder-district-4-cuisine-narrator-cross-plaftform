using System.ComponentModel.DataAnnotations;

namespace WebAdmin.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Display(Name = "Địa chỉ Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }

        /// <summary>
        /// Error message displayed after failed login attempt
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// URL to redirect after successful login (optional)
        /// </summary>
        public string? ReturnUrl { get; set; }
    }
}
