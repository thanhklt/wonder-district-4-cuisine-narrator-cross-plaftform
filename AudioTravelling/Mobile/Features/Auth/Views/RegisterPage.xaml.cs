using AudioTravelling.Mobile.Services.Api.Interfaces;
using AudioTravelling.Mobile.Services.Api.Requests;

namespace AudioTravelling.Mobile.Features.Auth.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void OnRegisterTapped(object sender, TappedEventArgs e)
    {
        LblError.IsVisible = false;
        LblError.Text = string.Empty;

        string fullName = EntryFullName.Text?.Trim() ?? string.Empty;
        string email = EntryEmail.Text?.Trim() ?? string.Empty;
        string password = EntryPassword.Text ?? string.Empty;
        string confirmPassword = EntryConfirmPassword.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(confirmPassword))
        {
            ShowError("Vui lòng nhập đầy đủ thông tin.");
            return;
        }

        if (password != confirmPassword)
        {
            ShowError("Mật khẩu xác nhận không khớp.");
            return;
        }

        if (password.Length < 6)
        {
            ShowError("Mật khẩu phải có ít nhất 6 ký tự.");
            return;
        }

        var authApiService = Handler?.MauiContext?.Services.GetService<IAuthApiService>();

        if (authApiService is null)
        {
            ShowError("Hệ thống đang tạm thời gặp sự cố. Vui lòng thử lại sau.");
            return;
        }

        try
        {
            SetLoadingState(true);

            var request = new RegisterRequest
            {
                FullName = fullName,
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword
            };

            var response = await authApiService.RegisterAsync(request);

            if (response is not null)
            {
                await DisplayAlert("Đăng ký thành công", "Tài khoản của bạn đã được tạo thành công.", "OK");

                if (Shell.Current is not null)
                    await Shell.Current.GoToAsync("..");
                else
                    await Navigation.PopAsync();

                return;
            }

            ShowError("Đăng ký chưa thành công. Vui lòng thử lại.");
        }
        catch (Exception ex)
        {
            ShowError(GetFriendlyErrorMessage(ex));
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private async void OnBackTapped(object sender, TappedEventArgs e)
    {
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync("..");
        else
            await Navigation.PopAsync();
    }

    private void ShowError(string message)
    {
        LblError.Text = message;
        LblError.IsVisible = true;
    }

    private void SetLoadingState(bool isLoading)
    {
        BtnRegisterBox.IsEnabled = !isLoading;
        Spinner.IsVisible = isLoading;
        Spinner.IsRunning = isLoading;
    }

    private static string GetFriendlyErrorMessage(Exception ex)
    {
        var message = ex.Message.ToLowerInvariant();

        if (message.Contains("email") && message.Contains("exist"))
            return "Email này đã được sử dụng.";

        if (message.Contains("400"))
            return "Thông tin đăng ký chưa hợp lệ.";

        if (message.Contains("401"))
            return "Bạn không có quyền thực hiện thao tác này.";

        if (message.Contains("403"))
            return "Yêu cầu bị từ chối.";

        if (message.Contains("404"))
            return "Không tìm thấy dịch vụ đăng ký.";

        if (message.Contains("500"))
            return "Máy chủ đang gặp sự cố. Vui lòng thử lại sau.";

        if (message.Contains("network") || message.Contains("connection") || message.Contains("socket"))
            return "Kết nối mạng không ổn định. Vui lòng kiểm tra lại mạng.";

        return "Đăng ký thất bại. Vui lòng thử lại sau.";
    }
}