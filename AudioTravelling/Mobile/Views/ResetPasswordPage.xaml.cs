using Mobile.Services;

namespace Mobile.Views;

public partial class ResetPasswordPage : ContentPage
{
    private string _email;

    public ResetPasswordPage(string email)
    {
        InitializeComponent();
        _email = email;
    }

    private async void OnBackToLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private async void OnUpdatePasswordTapped(object sender, EventArgs e)
    {
        // 1. Reset error state
        LblNewPasswordError.IsVisible = false;
        LblConfirmPasswordError.IsVisible = false;
        LblGeneralError.IsVisible = false;

        string newPassword = EntryNewPassword.Text ?? string.Empty;
        string confirmPassword = EntryConfirmPassword.Text ?? string.Empty;

        // 2. Inline validation
        bool valid = true;

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            LblNewPasswordError.Text = "Mật khẩu phải có ít nhất 6 ký tự";
            LblNewPasswordError.IsVisible = true;
            valid = false;
        }

        if (newPassword != confirmPassword)
        {
            LblConfirmPasswordError.Text = "Mật khẩu không khớp";
            LblConfirmPasswordError.IsVisible = true;
            valid = false;
        }

        if (!valid) return;

        // 3. Call Service to handle Save Password
        var (success, errorMsg) = AuthService.SaveNewPassword(_email, newPassword);

        if (!success)
        {
            LblGeneralError.Text = errorMsg;
            LblGeneralError.IsVisible = true;
            return;
        }

        // 4. Success -> Show alert and go to Login page
        await DisplayAlertAsync("Thành công", "Mật khẩu đã được thay đổi thành công. Bạn có thể đăng nhập bằng mật khẩu mới ngay bây giờ.", "Đăng nhập ngay");
        
        // Pop back to root (which should be the login page based on our stack flow)
        await Navigation.PopToRootAsync();
    }
}
