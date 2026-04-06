using Mobile.Models;

namespace Mobile.Services;

/// <summary>
/// Static dummy auth service for MVP. Replace with real API in production.
/// </summary>
public static class AuthService
{
    // ── Session state ──────────────────────────────────────────────────
    public static bool   IsLoggedIn  { get; private set; } = false;
    public static string CurrentName { get; private set; } = string.Empty;
    public static string CurrentEmail{ get; private set; } = string.Empty;
    public static string CurrentPhone{ get; private set; } = string.Empty;

    // ── Login ──────────────────────────────────────────────────────────
    // ── Default test account ───────────────────────────────────────────
    public const string TestEmail    = "test@gmail.com";
    public const string TestPassword = "123456";

    /// <summary>
    /// Dummy login: accepts any valid email + password ≥ 6 chars.
    /// Returns (success, errorMessage).
    /// </summary>
    public static (bool Success, string Error) Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (false, "Email không được để trống");

        if (!email.Contains('@') || !email.Contains('.'))
            return (false, "Email không hợp lệ");

        if (string.IsNullOrWhiteSpace(password))
            return (false, "Mật khẩu không được để trống");

        if (password.Length < 6)
            return (false, "Mật khẩu phải có ít nhất 6 ký tự");

        // MVP: accept any valid format
        IsLoggedIn   = true;
        CurrentEmail = email;
        CurrentName  = email.Split('@')[0]; // derive name from email for dummy
        // Don't modify CurrentPhone on login since we don't have it, keep existing or set empty
        if (string.IsNullOrEmpty(CurrentPhone)) CurrentPhone = string.Empty; 
        return (true, string.Empty);
    }

    // ── Register ───────────────────────────────────────────────────────
    /// <summary>
    /// Dummy register: validates fields, always succeeds.
    /// Returns (success, errorMessage).
    /// </summary>
    public static (bool Success, string Error) Register(
        string fullName, string email, string phone, string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return (false, "Họ tên không được để trống");

        if (string.IsNullOrWhiteSpace(email))
            return (false, "Email không được để trống");

        if (!email.Contains('@') || !email.Contains('.'))
            return (false, "Email không hợp lệ");

        if (string.IsNullOrWhiteSpace(phone))
            return (false, "Số điện thoại không được để trống");

        if (phone.Length < 10 || phone.Length > 11 || !phone.StartsWith("0"))
            return (false, "Số điện thoại không hợp lệ (10-11 số, bắt đầu bằng 0)");

        if (string.IsNullOrWhiteSpace(password))
            return (false, "Mật khẩu không được để trống");

        if (password.Length < 6)
            return (false, "Mật khẩu phải có ít nhất 6 ký tự");

        if (password != confirmPassword)
            return (false, "Mật khẩu không khớp");

        // Save on register success
        CurrentName = fullName;
        CurrentPhone = phone;
        CurrentEmail = email;

        return (true, string.Empty);
    }

    // ── Update Profile ─────────────────────────────────────────────────
    public static (bool Success, string Error) UpdateProfile(string fullName, string phone)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return (false, "Họ tên không được để trống");

        if (string.IsNullOrWhiteSpace(phone))
            return (false, "Số điện thoại không được để trống");

        if (phone.Length < 10 || phone.Length > 11 || !phone.StartsWith("0"))
            return (false, "Số điện thoại không hợp lệ (10-11 số, bắt đầu bằng 0)");

        CurrentName = fullName;
        CurrentPhone = phone;
        return (true, string.Empty);
    }

    // ── Forgot Password ────────────────────────────────────────────────
    public static (bool Success, string Error) ResetPassword(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (false, "Email không được để trống");

        if (!email.Contains('@') || !email.Contains('.'))
            return (false, "Email không hợp lệ");

        // Dummy success
        return (true, string.Empty);
    }

    public static (bool Success, string Error) SaveNewPassword(string email, string newPassword)
    {
        // Dummy success since we don't have a real DB
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            return (false, "Mật khẩu không hợp lệ");

        return (true, string.Empty);
    }

    public static void Logout()
    {
        IsLoggedIn   = false;
        CurrentName  = string.Empty;
        CurrentEmail = string.Empty;
        CurrentPhone = string.Empty;
        CartService.Instance.Clear();
    }
}
