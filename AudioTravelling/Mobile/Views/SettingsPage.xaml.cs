using Mobile.Services;

namespace Mobile.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        // Sync toggle to current global state
        AudioToggle.IsToggled = PoiService.AudioEnabled;
        UpdateAudioStatusLabel(PoiService.AudioEnabled);
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

    // ── Language picker ────────────────────────────────────────────────────
    private void SetLang(Border selected, params Border[] others)
    {
        selected.BackgroundColor = Color.FromArgb("#10B981");
        foreach (var b in others) b.BackgroundColor = Colors.White;
    }

    private void OnLangViTapped(object sender, EventArgs e) =>
        SetLang(LangViBtn, LangEnBtn, LangJaBtn);

    private void OnLangEnTapped(object sender, EventArgs e) =>
        SetLang(LangEnBtn, LangViBtn, LangJaBtn);

    private void OnLangJaTapped(object sender, EventArgs e) =>
        SetLang(LangJaBtn, LangViBtn, LangEnBtn);
}
