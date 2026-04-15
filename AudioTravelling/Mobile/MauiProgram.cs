using AudioTravelling.Mobile.Features.Auth.ViewModels;
using AudioTravelling.Mobile.Features.Auth.Views;
using AudioTravelling.Mobile.Services.Api;
using AudioTravelling.Mobile.Services.Api.Handlers;
using AudioTravelling.Mobile.Services.Api.Interfaces;
using AudioTravelling.Mobile.Services.Auth;

using AudioTravelling.Mobile.Core.Sync;
using AudioTravelling.Mobile.Data.SQLite;
using AudioTravelling.Mobile.Data.SQLite.Services;

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
            })
            .UseMauiMaps();

        builder.Services.AddSingleton<ITokenService, TokenService>();
        builder.Services.AddSingleton<LoggingHandler>();
        builder.Services.AddSingleton<AuthHeaderHandler>();

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

        builder.Services.AddHttpClient<ISyncApiService, SyncApiService>(client =>
        {
            client.BaseAddress = new Uri(ApiOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<LoggingHandler>()
        .AddHttpMessageHandler<AuthHeaderHandler>();

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "audiotravelling.db");

        builder.Services.AddSingleton(new AppDbContext(dbPath));
        builder.Services.AddSingleton<ICacheDbService, CacheDbService>();
        builder.Services.AddSingleton<ISyncService, SyncService>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<RegisterPage>();

        return builder.Build();
    }
}