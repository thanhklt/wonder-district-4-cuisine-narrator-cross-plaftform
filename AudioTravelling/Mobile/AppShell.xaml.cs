using AudioTravelling.Mobile.Features.Auth.Views;
using AudioTravelling.Mobile.Features.Map.Views;

namespace AudioTravelling.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        }
    }
}
