using AudioTravelling.Mobile.Core.Sync;
using AudioTravelling.Mobile.Data.SQLite;
using AudioTravelling.Mobile.Data.SQLite.Services;
using AudioTravelling.Mobile.Features.Audio.Services;
using AudioTravelling.Mobile.Features.Audio.Views;
using AudioTravelling.Mobile.Features.Auth.ViewModels;
using AudioTravelling.Mobile.Features.Auth.Views;
using AudioTravelling.Mobile.Features.Map.Views;
using AudioTravelling.Mobile.Features.Order.Views;
using AudioTravelling.Mobile.Features.Poi.ViewModels;
using AudioTravelling.Mobile.Features.Poi.Views;
using AudioTravelling.Mobile.Features.Settings.Views;
using AudioTravelling.Mobile.Services.Api;
using AudioTravelling.Mobile.Services.Api.Handlers;
using AudioTravelling.Mobile.Services.Api.Interfaces;
using AudioTravelling.Mobile.Services.Auth;

namespace AudioTravelling.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("fa-solid-900.ttf", "FA6Solid");
            })
            .UseMauiMaps();

        // Core services
        builder.Services.AddSingleton<ITokenService, TokenService>();
        builder.Services.AddSingleton<ITextToSpeechService, TextToSpeechService>();

        // Handlers: nên để Transient
        builder.Services.AddTransient<LoggingHandler>();
        builder.Services.AddTransient<AuthHeaderHandler>();

        // Local DB
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "audiotravelling.db");
        builder.Services.AddSingleton(new AppDbContext(dbPath));
        builder.Services.AddSingleton<ICacheDbService, CacheDbService>();

        // Sync
        builder.Services.AddHttpClient<ISyncApiService, SyncApiService>(client =>
        {
            client.BaseAddress = new Uri(ApiOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<LoggingHandler>()
        .AddHttpMessageHandler<AuthHeaderHandler>();

        builder.Services.AddScoped<ISyncService, SyncService>();

        // API services
        builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(client =>
        {
            client.BaseAddress = new Uri(ApiOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<LoggingHandler>()
        .AddHttpMessageHandler<AuthHeaderHandler>();

        builder.Services.AddHttpClient<IUserApiService, UserApiService>(client =>
        {
            client.BaseAddress = new Uri(ApiOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<LoggingHandler>()
        .AddHttpMessageHandler<AuthHeaderHandler>();

        builder.Services.AddHttpClient<IPoiApiService, PoiApiService>(client =>
        {
            client.BaseAddress = new Uri(ApiOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<LoggingHandler>();

        builder.Services.AddHttpClient<IAudioApiService, AudioApiService>(client =>
        {
            client.BaseAddress = new Uri(ApiOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<LoggingHandler>()
        .AddHttpMessageHandler<AuthHeaderHandler>();

        builder.Services.AddHttpClient("DefaultClient");

        // Audio Services
        builder.Services.AddSingleton<IAudioCacheService, AudioCacheService>();
        builder.Services.AddSingleton<IAudioPlaybackManager, AudioPlaybackManager>();

        // Auth
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<RegisterPage>();

        // Shell + pages
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<OrderPage>();
        builder.Services.AddTransient<AudioPlayerPage>();
        builder.Services.AddTransient<SettingsPage>();

        // POI
        builder.Services.AddTransient<PoiListViewModel>();
        builder.Services.AddTransient<PoiListPage>();
        builder.Services.AddTransient<PoiDetailViewModel>();
        builder.Services.AddTransient<PoiDetailPage>();

        // Audio trigger - now managed by AudioPlaybackManager
        // builder.Services.AddScoped<IAudioTriggerService, AudioTriggerService>();

        return builder.Build();
    }
}