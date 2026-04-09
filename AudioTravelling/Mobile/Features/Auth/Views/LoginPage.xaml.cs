using AudioTravelling.Mobile.Features.Auth.Services;

namespace AudioTravelling.Mobile.Features.Auth.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();

        // Pre-fill with test account for easy testing
        EntryEmail.Text    = AuthService.TestEmail;
        EntryPassword.Text = AuthService.TestPassword;
    }

    private async void OnLoginTapped(object sender, EventArgs e)
    {
        // Reset errors
        LblEmailError.IsVisible    = false;
        LblPasswordError.IsVisible = false;
        LblGeneralError.IsVisible  = false;

        string email    = EntryEmail.Text?.Trim() ?? "";
        string password = EntryPassword.Text ?? "";

        // Inline validation
        bool valid = true;

        if (string.IsNullOrWhiteSpace(email))
        {
            LblEmailError.Text      = "Không được để trống";
            LblEmailError.IsVisible = true;
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            LblPasswordError.Text      = "Không được để trống";
            LblPasswordError.IsVisible = true;
            valid = false;
        }

        if (!valid) return;

        // Call auth service
        var (success, error) = AuthService.Login(email, password);

        if (success)
        {
            // Navigate to main app
            Application.Current!.MainPage = new AppShell();
        }
        else
        {
            LblGeneralError.Text      = error;
            LblGeneralError.IsVisible = true;
        }
    }

    private async void OnRegisterLinkTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }

    private async void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ForgotPasswordPage());
    }
}
