using AudioTravelling.Mobile.Features.Audio.Services;
using AudioTravelling.Mobile.Features.Audio.ViewModels;
using AudioTravelling.Mobile.Features.Audio.Views;
using AudioTravelling.Mobile.Features.Map.Views;
using AudioTravelling.Mobile.Features.Notification.Views;
using AudioTravelling.Mobile.Features.Order.Views;
using AudioTravelling.Mobile.Features.Settings.Views;
using Plugin.Maui.Audio;

namespace AudioTravelling.Mobile
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
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSans-Regular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSans-Semibold");
                    fonts.AddFont("fa-solid-900.ttf", "FA6Solid");
                    fonts.AddFont("OpenSans-Bold.ttf", "OpenSans-Bold");
                });

            // ── Services ──────────────────────────────────────────────────
            builder.Services.AddSingleton(AudioManager.Current);
            builder.Services.AddSingleton<AudioService>();
            builder.Services.AddSingleton<ITextToSpeechService, TextToSpeechService>();
            builder.Services.AddTransient<AudioViewModel>();
            builder.Services.AddTransient<AudioPlayerPage>();

            // ── Routes ────────────────────────────────────────────────────
            Routing.RegisterRoute("OrderPage", typeof(OrderPage));
            Routing.RegisterRoute("MainPage", typeof(MainPage));
            Routing.RegisterRoute("AudioPlayerPage", typeof(AudioPlayerPage));
            Routing.RegisterRoute("SettingsPage", typeof(SettingsPage));
            Routing.RegisterRoute("NotificationsPage", typeof(NotificationsPage));

            return builder.Build();
        }
    }
}
