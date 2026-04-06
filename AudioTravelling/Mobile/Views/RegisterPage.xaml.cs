using Mobile.Services;

namespace Mobile.Views;

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
        string phone           = EntryPhone.Text?.Trim() ?? "";
        string password        = EntryPassword.Text ?? "";
        string confirmPassword = EntryConfirmPassword.Text ?? "";

        var (success, error) = AuthService.Register(fullName, email, phone, password, confirmPassword);

        if (success)
        {
            await DisplayAlertAsync("Đăng ký thành công!", "Vui lòng đăng nhập để tiếp tục.", "OK");
            await Navigation.PopAsync();
        }
        else
        {
            LblError.Text      = error;
            LblError.IsVisible = true;
        }
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
