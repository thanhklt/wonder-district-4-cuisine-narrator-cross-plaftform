using AudioTravelling.Mobile.Features.Auth.Views;
using AudioTravelling.Mobile.Features.Map.Views;
using AudioTravelling.Mobile.Features.Poi.Views;
using AudioTravelling.Mobile.Features.Order.Views;
using AudioTravelling.Mobile.Features.Audio.Views;
using AudioTravelling.Mobile.Features.Settings.Views;

namespace AudioTravelling.Mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Auth
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));

            // Tab pages
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(PoiListPage), typeof(PoiListPage));
            Routing.RegisterRoute(nameof(OrderPage), typeof(OrderPage));
            Routing.RegisterRoute(nameof(AudioPlayerPage), typeof(AudioPlayerPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));

            // Detail pages
            Routing.RegisterRoute(nameof(PoiDetailPage), typeof(PoiDetailPage));
        }
    }
}