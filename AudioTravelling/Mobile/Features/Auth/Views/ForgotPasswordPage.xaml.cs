using AudioTravelling.Mobile.Features.Auth.Services;

namespace AudioTravelling.Mobile.Features.Auth.Views;

public partial class ForgotPasswordPage : ContentPage
{
    public ForgotPasswordPage()
    {
        InitializeComponent();
    }

    private async void OnBackToLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSendResetLinkTapped(object sender, EventArgs e)
    {
        LblEmailError.IsVisible = false;
        LblGeneralError.IsVisible = false;

        string email = EntryEmail.Text?.Trim() ?? string.Empty;

        var (success, errorMsg) = AuthService.ResetPassword(email);

        if (!success)
        {
            if (errorMsg.Contains("Email"))
            {
                LblEmailError.Text = errorMsg;
                LblEmailError.IsVisible = true;
            }
            else
            {
                LblGeneralError.Text = errorMsg;
                LblGeneralError.IsVisible = true;
            }
            return;
        }

        await DisplayAlertAsync("Thông báo (Demo)", "Một liên kết khôi phục đã được gửi. Ứng dụng sẽ tự động chuyển sang trang Tạo mật khẩu mới (Mô phỏng thao tác người dùng bấm vào link xác nhận trong Email).", "Mở trang Tạo Mật Khẩu");
        
        await Navigation.PushAsync(new ResetPasswordPage(email));
    }
}
