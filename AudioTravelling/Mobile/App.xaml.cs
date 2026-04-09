using AudioTravelling.Mobile.Features.Auth.Services;
using AudioTravelling.Mobile.Features.Auth.Views;

namespace AudioTravelling.Mobile
{
    public partial class App : Application
    {
        [Obsolete]
        public App()
        {
            InitializeComponent();

            if (AuthService.IsLoggedIn)
                MainPage = new AppShell();
            else
                MainPage = new NavigationPage(new LoginPage());
        }
    }
}