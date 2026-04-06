using Mobile.Services;
using Mobile.Models;

namespace Mobile.Views;

public partial class SettingsPage : ContentPage
{
    // ── Playback speed state (BR-012) ──────────────────────────────────
    private int _speedIndex = 1;
    private readonly double[] _speeds = { 0.75, 1.0, 1.25, 1.5, 2.0 };
   

    public SettingsPage()
    {
        InitializeComponent();
        // Sync toggle to current global state
        AudioToggle.IsToggled = PoiService.AudioEnabled;
        UpdateAudioStatusLabel(PoiService.AudioEnabled);

        // Restore persisted speed
        var savedSpeed = Preferences.Get("playback_speed", 1.0);
        _speedIndex = Array.IndexOf(_speeds, savedSpeed);
        if (_speedIndex < 0) _speedIndex = 1;
        
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadUserProfile();
    }

    // ── User profile ──────────────────────────────────────────────────────
    private void LoadUserProfile()
    {
        if (AuthService.IsLoggedIn)
        {
            LblUserName.Text  = AuthService.CurrentName;
            LblUserEmail.Text = AuthService.CurrentEmail;
            LblAvatar.Text    = AuthService.CurrentName.Length > 0
                ? AuthService.CurrentName[..1].ToUpper()
                : "U";
        }
    }

    private async void OnUserProfileTapped(object sender, EventArgs e)
    {
        if (AuthService.IsLoggedIn)
        {
            await Navigation.PushAsync(new ProfilePage());
        }
    }

    // ── Audio toggle ────────────────────────────────────────────────────────
    private void OnAudioToggled(object sender, ToggledEventArgs e)
    {
        PoiService.AudioEnabled = e.Value;
        UpdateAudioStatusLabel(e.Value);
    }

    private void UpdateAudioStatusLabel(bool enabled)
    {
        LblAudioStatus.Text      = enabled
            ? "Đang bật – phát khi đến gần POI"
            : "Đã tắt – không phát âm thanh";
        LblAudioStatus.TextColor = enabled
            ? Color.FromArgb("#10B981")
            : Color.FromArgb("#EF4444");
    }

    // ── Playback speed picker (BR-012) ──────────────────────────────────
    private void OnSpeedTapped(object sender, EventArgs e)
    {
        _speedIndex = (_speedIndex + 1) % _speeds.Length;
        Preferences.Set("playback_speed", _speeds[_speedIndex]);
    }

    // ── Logout ────────────────────────────────────────────────────────────
    private async void OnLogoutTapped(object sender, EventArgs e)
    {
        string message = "Bạn có chắc chắn muốn đăng xuất?";

        // EC-S3: Warn if cart has items
        if (CartService.Instance.TotalCount > 0)
            message = $"Bạn có {CartService.Instance.TotalCount} món trong giỏ hàng. Đăng xuất sẽ xóa giỏ hàng. Tiếp tục?";

        bool confirm = await DisplayAlertAsync("Đăng xuất", message, "Đăng xuất", "Hủy");
        if (!confirm) return;

        AuthService.Logout();
        Application.Current!.MainPage = new NavigationPage(new LoginPage());
    }
}

