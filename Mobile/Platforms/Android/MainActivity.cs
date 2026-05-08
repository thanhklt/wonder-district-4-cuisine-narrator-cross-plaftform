using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Mobile.Services;
using Mobile.ViewModels;

namespace Mobile;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
                           ConfigChanges.UiMode | ConfigChanges.ScreenLayout |
                           ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[IntentFilter(
    [Intent.ActionView],
    Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
    DataScheme = "audiotravelling",
    DataHost = "session")]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        HandleDeepLinkIntent(Intent);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        HandleDeepLinkIntent(intent);
    }

    protected override void OnResume()
    {
        base.OnResume();
        _ = EnsureForegroundServiceRunningAsync();
    }

    private static void HandleDeepLinkIntent(Intent? intent)
    {
        if (intent?.Data?.Scheme != "audiotravelling") return;
        if (intent.Data?.Host != "session") return;

        var sessionIdStr = intent.Data.GetQueryParameter("sessionId");
        var deviceId = intent.Data.GetQueryParameter("deviceId");

        if (!int.TryParse(sessionIdStr, out var sessionId) || string.IsNullOrEmpty(deviceId))
            return;

        var expiredAt = DateTime.UtcNow.AddHours(24);

        Task.Run(async () =>
        {
            var sessionSvc = IPlatformApplication.Current?.Services.GetService<SessionService>();
            if (sessionSvc is null) return;

            await sessionSvc.SaveSessionAsync(sessionId, expiredAt);
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.GoToAsync("//map"));
        });
    }

    private static async Task EnsureForegroundServiceRunningAsync()
    {
        var sessionSvc = IPlatformApplication.Current?.Services.GetService<SessionService>();
        if (sessionSvc is null) return;
        if (await sessionSvc.GetValidSessionAsync() is null) return;

        await GeofenceServiceStarter.StartAsync(requestPermission: false);
    }
}
