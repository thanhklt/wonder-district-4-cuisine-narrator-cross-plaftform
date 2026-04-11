using AudioTravelling.Mobile.Features.Auth.Services;

namespace AudioTravelling.Mobile.Features.Auth.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void OnRegisterTapped(object sender, EventArgs e)
    {
        LblError.IsVisible = false;

        string fullName        = EntryFullName.Text?.Trim() ?? "";
        string email           = EntryEmail.Text?.Trim() ?? "";
        string password        = EntryPassword.Text ?? "";
        string confirmPassword = EntryConfirmPassword.Text ?? "";

        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            LblError.Text = "Vui lòng nhập đầy đủ thông tin.";
            LblError.IsVisible = true;
            return;
        }

        if (password != confirmPassword)
        {
            LblError.Text = "Mật khẩu không khớp.";
            LblError.IsVisible = true;
            return;
        }
        
        if (password.Length < 6)
        {
            LblError.Text = "Mật khẩu phải có ít nhất 6 ký tự.";
            LblError.IsVisible = true;
            return;
        }

        var authApiService = this.Handler?.MauiContext?.Services.GetService<AudioTravelling.Mobile.Services.Api.Interfaces.IAuthApiService>();
        
        if (authApiService == null)
        {
            LblError.Text = "Lỗi hệ thống: Không tìm thấy dịch vụ xác thực.";
            LblError.IsVisible = true;
            return;
        }

        try
        {
            BtnRegisterBox.IsEnabled = false;
            Spinner.IsRunning = true;
            Spinner.IsVisible = true;

            var request = new AudioTravelling.Mobile.Services.Api.Requests.RegisterRequest
            {
                FullName = fullName,
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword
            };

            var response = await authApiService.RegisterAsync(request);

            if (response != null)
            {
                await DisplayAlertAsync("Đăng ký thành công!", "Tài khoản của bạn đã được tạo thành công.", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                LblError.Text = "Đăng ký thất bại. Vui lòng thử lại.";
                LblError.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            LblError.Text = $"Đăng ký thất bại: {ex.Message}";
            LblError.IsVisible = true;
        }
        finally
        {
            BtnRegisterBox.IsEnabled = true;
            Spinner.IsRunning = false;
            Spinner.IsVisible = false;
        }
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
