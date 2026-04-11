using AudioTravelling.Mobile.Features.Auth.Views;

namespace AudioTravelling.Mobile;

public partial class App : Application
{
    public App(LoginPage loginPage)
    {
        InitializeComponent();

        // Always show LoginPage when app starts
        MainPage = loginPage;
    }
}