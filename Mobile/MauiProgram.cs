using Microsoft.Extensions.Logging;
using Mobile.Services;
using Mobile.ViewModels;
using Mobile.Views;
using Plugin.Maui.Audio;
using SkiaSharp.Views.Maui.Controls.Hosting;
using ZXing.Net.Maui.Controls;

namespace Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseBarcodeReader()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Services (singleton để dùng chung state)
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<SessionService>();
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddSingleton<AudioService>();

        // ViewModels
        builder.Services.AddTransient<SplashViewModel>();
        builder.Services.AddTransient<QrScanViewModel>();
        builder.Services.AddSingleton<MapViewModel>();
        builder.Services.AddTransient<PoiDetailViewModel>();

        // Pages
        builder.Services.AddTransient<SplashPage>();
        builder.Services.AddTransient<QrScanPage>();
        builder.Services.AddSingleton<MapPage>();
        builder.Services.AddTransient<PoiDetailPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // In ra tat ca unhandled exception de de debug JavaProxyThrowable
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            System.Diagnostics.Debug.WriteLine($"[UnhandledException] {e.ExceptionObject}");
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"[UnobservedTask] {e.Exception}");
            e.SetObserved();
        };

        return app;
    }
}
