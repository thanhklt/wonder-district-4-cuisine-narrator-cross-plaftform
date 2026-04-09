using AudioTravelling.Mobile.Features.Auth.Services;
using AudioTravelling.Mobile.Features.Auth.Views;
using AudioTravelling.Mobile.Features.Poi.Services;

namespace AudioTravelling.Mobile.Features.Settings.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        LblUserName.Text  = AuthService.CurrentName;
        LblUserEmail.Text = AuthService.CurrentEmail;

        LblAvatar.Text = AuthService.CurrentName.Length > 0
            ? AuthService.CurrentName[..1].ToUpper()
            : "U";

        AudioToggle.IsToggled = PoiService.AudioEnabled;
        UpdateAudioStatusLabel(PoiService.AudioEnabled);
    }

    private void OnAudioToggled(object sender, ToggledEventArgs e)
    {
        PoiService.AudioEnabled = e.Value;
        UpdateAudioStatusLabel(e.Value);
    }

    private void UpdateAudioStatusLabel(bool enabled)
    {
        LblAudioStatus.Text = enabled
            ? "Đang bật – phát khi đến gần POI"
            : "Đã tắt – không tự phát audio";
        LblAudioStatus.TextColor = enabled
            ? Color.FromArgb("#10B981")
            : Color.FromArgb("#EF4444");
    }

    private async void OnUserProfileTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ProfilePage());
    }

    private async void OnLogoutTapped(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Đăng xuất", "Bạn có chắc muốn đăng xuất không?", "Đăng xuất", "Hủy");
        if (!confirm) return;

        AuthService.Logout();
        Application.Current!.MainPage = new NavigationPage(new LoginPage());
    }
}
