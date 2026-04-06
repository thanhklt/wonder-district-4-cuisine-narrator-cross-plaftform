using Microsoft.Extensions.DependencyInjection;
using Mobile.Services;
using Mobile.Views;

namespace Mobile
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