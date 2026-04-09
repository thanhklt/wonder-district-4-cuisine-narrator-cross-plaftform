using AudioTravelling.Mobile.Features.Auth.Services;

namespace AudioTravelling.Mobile.Features.Auth.Views;

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
        LblNewPasswordError.IsVisible = false;
        LblConfirmPasswordError.IsVisible = false;
        LblGeneralError.IsVisible = false;

        string newPassword = EntryNewPassword.Text ?? string.Empty;
        string confirmPassword = EntryConfirmPassword.Text ?? string.Empty;

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

        var (success, errorMsg) = AuthService.SaveNewPassword(_email, newPassword);

        if (!success)
        {
            LblGeneralError.Text = errorMsg;
            LblGeneralError.IsVisible = true;
            return;
        }

        await DisplayAlertAsync("Thành công", "Mật khẩu đã được thay đổi thành công. Bạn có thể đăng nhập bằng mật khẩu mới ngay bây giờ.", "Đăng nhập ngay");
        
        await Navigation.PopToRootAsync();
    }
}
