using Microsoft.Extensions.Logging;
using Mobile.Services;
using Plugin.Maui.Audio;

namespace Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiMaps()
                .ConfigureFonts(fonts =>
                {
                    // Register with both alias conventions:
                    // "OpenSansRegular" / "OpenSansSemibold" → used in Styles.xaml
                    // "OpenSans-Regular" / "OpenSans-Semibold" → used in MainPage.xaml
                    fonts.AddFont("OpenSans-Regular.ttf",  "OpenSansRegular");
                    fonts.AddFont("OpenSans-Regular.ttf",  "OpenSans-Regular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSans-Semibold");
                    // FontAwesome 6 Free – Solid  →  place fa-solid-900.ttf in Resources/Fonts/
                    fonts.AddFont("fa-solid-900.ttf", "FA6Solid");
                });

            // Views
            Routing.RegisterRoute("OrderPage", typeof(Views.OrderPage));
            Routing.RegisterRoute("MainPage", typeof(Views.MainPage));
            Routing.RegisterRoute("AudioPlayerPage", typeof(Views.AudioPlayerPage));
            Routing.RegisterRoute("SettingsPage", typeof(Views.SettingsPage));
            Routing.RegisterRoute("NotificationsPage", typeof(Views.NotificationsPage));

            builder.Services.AddSingleton<AudioService>();
            builder.Services.AddSingleton(AudioManager.Current);

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
