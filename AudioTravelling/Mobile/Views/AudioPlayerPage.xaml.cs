using Mobile.Models;
using Mobile.Services;
using Microsoft.Maui.Controls.Shapes;

namespace Mobile.Views;

public partial class AudioPlayerPage : ContentPage
{
    // ── State ──────────────────────────────────────────────────────────
    private bool   _isPlaying     = false;
    private bool   _trackListOpen = false;
    private int    _speedIndex    = 1; // index into _speeds
    private readonly double[] _speeds = { 0.75, 1.0, 1.25, 1.5, 2.0 };

    private PoiModel? _currentPoi;

    public AudioPlayerPage()
    {
        InitializeComponent();

        // Load first POI as default audio
        var pois = PoiService.GetSortedByDistance();
        if (pois.Count > 0)
            LoadPoi(pois[0]);

        BuildTrackList();

        // Listen to audio toggle from Settings
        PoiService.AudioEnabledChanged += OnAudioEnabledChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (!PoiService.AudioEnabled)
        {
            _isPlaying = false;
            PlayPauseIcon.Text = "\uf04b";
        }
    }

    // ── Load POI audio data ────────────────────────────────────────────
    private void LoadPoi(PoiModel poi)
    {
        _currentPoi            = poi;
        HeroImage.Source        = poi.ImageSource;
        LblTrackTitle.Text     = poi.Name;
        LblTrackSubtitle.Text  = $"{poi.Category} • Dotonbori";
        LblTranscript.Text     = poi.AudioScript;
        LblCurrentTime.Text    = "0:00";
        LblTotalTime.Text      = "3:45"; // dummy
        AudioProgress.Progress = 0;
        _isPlaying             = false;
        PlayPauseIcon.Text     = "\uf04b";
    }

    // ── Playback controls ──────────────────────────────────────────────
    private async void OnPlayPauseTapped(object sender, EventArgs e)
    {
        if (!PoiService.AudioEnabled)
        {
            await DisplayAlertAsync("Âm thanh đã tắt", "Vui lòng bật âm thanh trong Cài đặt.", "OK");
            return;
        }

        _isPlaying = !_isPlaying;
        PlayPauseIcon.Text = _isPlaying ? "\uf04c" : "\uf04b"; // pause : play

        if (sender is Border btn)
        {
            await btn.ScaleToAsync(0.88, 70, Easing.CubicOut);
            await btn.ScaleToAsync(1.0, 120, Easing.SpringOut);
        }
    }

    private void OnSpeedTapped(object sender, EventArgs e)
    {
        _speedIndex = (_speedIndex + 1) % _speeds.Length;
        LblSpeed.Text = $"{_speeds[_speedIndex]:F2}x".Replace(".00", ".0").Replace("0x", "x");
        // Format nicely
        double spd = _speeds[_speedIndex];
        LblSpeed.Text = spd == 1.0 ? "1.0x"
                       : spd == 0.75 ? "0.75x"
                       : spd == 1.25 ? "1.25x"
                       : spd == 1.5  ? "1.5x"
                       : "2.0x";
    }

    // ── Track list ─────────────────────────────────────────────────────
    private void OnTrackListTapped(object sender, EventArgs e)
    {
        _trackListOpen = !_trackListOpen;
        TrackListPanel.IsVisible    = _trackListOpen;
        LblTrackListHeader.IsVisible = _trackListOpen;
    }

    private void BuildTrackList()
    {
        var pois = PoiService.GetSortedByDistance();
        TrackListPanel.Children.Clear();

        foreach (var poi in pois)
        {
            var card = new Border
            {
                StrokeShape     = new RoundRectangle { CornerRadius = 14 },
                Stroke          = new SolidColorBrush(Color.FromArgb("#E2E8F0")),
                StrokeThickness = 1,
                BackgroundColor = Colors.White,
                Padding         = new Thickness(14),
                Shadow          = new Shadow { Brush = Brush.Black, Offset = new Point(0, 2), Opacity = 0.05f, Radius = 6 }
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = 48 },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                ColumnSpacing = 12
            };

            // Thumbnail
            var thumb = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                Stroke      = Brush.Transparent,
                WidthRequest  = 48,
                HeightRequest = 48,
                Content = new Image { Source = poi.ImageSource, Aspect = Aspect.AspectFill }
            };
            Grid.SetColumn(thumb, 0);
            grid.Children.Add(thumb);

            // Info
            var info = new VerticalStackLayout { Spacing = 3, VerticalOptions = LayoutOptions.Center };
            info.Children.Add(new Label { Text = poi.Name, FontFamily = "OpenSans-Semibold", FontSize = 14, TextColor = Color.FromArgb("#0F172A"), LineBreakMode = LineBreakMode.TailTruncation });
            info.Children.Add(new Label { Text = poi.Category, FontFamily = "OpenSans-Regular", FontSize = 12, TextColor = Color.FromArgb("#0F172A") });
            Grid.SetColumn(info, 1);
            grid.Children.Add(info);

            // Play icon
            var playIcon = new Label { Text = "\uf04b", FontFamily = "FA6Solid", FontSize = 14, TextColor = Colors.Black, VerticalOptions = LayoutOptions.Center };
            // Highlight if currently playing
            if (_currentPoi != null && _currentPoi.Id == poi.Id)
            {
                card.BackgroundColor = Color.FromArgb("#f1f1f1ff");
                playIcon.Text = "\uf028"; // volume icon
                playIcon.TextColor = Colors.Black;
            }
            Grid.SetColumn(playIcon, 2);
            grid.Children.Add(playIcon);

            card.Content = grid;

            var tap = new TapGestureRecognizer();
            tap.CommandParameter = poi;
            tap.Tapped += OnTrackItemTapped;
            card.GestureRecognizers.Add(tap);

            TrackListPanel.Children.Add(card);
        }
    }

    private void OnTrackItemTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not PoiModel poi) return;
        LoadPoi(poi);
        BuildTrackList(); // refresh highlighting
    }

    // ── Audio toggle listener ──────────────────────────────────────────
    private void OnAudioEnabledChanged(bool enabled)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!enabled)
            {
                _isPlaying = false;
                PlayPauseIcon.Text = "\uf04b";
            }
        });
    }
}
