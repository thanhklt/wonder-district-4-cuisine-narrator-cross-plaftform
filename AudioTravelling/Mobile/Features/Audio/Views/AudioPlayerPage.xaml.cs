using AudioTravelling.Mobile.Features.Audio.Services;
using AudioTravelling.Mobile.Features.Poi.Models;
using AudioTravelling.Mobile.Features.Poi.Services;
using Microsoft.Maui.Controls.Shapes;

namespace AudioTravelling.Mobile.Features.Audio.Views;

public partial class AudioPlayerPage : ContentPage
{
    private bool   _trackListOpen = false;
    private int    _speedIndex    = 1;
    private readonly double[] _speeds = { 0.75, 1.0, 1.25, 1.5, 2.0 };

    private PoiModel? _currentPoi;

    private readonly AudioPlaybackService _playback = AudioPlaybackService.Instance;
    private readonly ITextToSpeechService _ttsService;
    private bool _isSpeaking = false;

    public AudioPlayerPage(ITextToSpeechService ttsService)
    {
        _ttsService = ttsService;
        InitializeComponent();

        var pois = PoiService.GetSortedByDistance();
        if (pois.Count > 0)
            LoadPoi(pois[0]);

        BuildTrackList();

        PoiService.AudioEnabledChanged += OnAudioEnabledChanged;

        _playback.StateChanged    += OnPlaybackStateChanged;
        _playback.ProgressChanged += OnPlaybackProgressChanged;
        _playback.PlaybackEnded   += OnPlaybackEnded;
        _playback.ErrorOccurred   += OnPlaybackError;
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (!PoiService.AudioEnabled)
        {
            _ = _playback.StopAsync();
            PlayPauseIcon.Text = "\uf04b";
        }

        UpdatePlaybackUI();
    }

    private void LoadPoi(PoiModel poi)
    {
        _currentPoi            = poi;
        HeroImage.Source        = poi.ImageSource;
        LblTrackTitle.Text     = poi.Name;
        LblTrackSubtitle.Text  = $"{poi.Category} • Dotonbori";
        LblTranscript.Text     = poi.AudioScript;
        LblCurrentTime.Text    = "0:00";
        LblTotalTime.Text      = "0:00";
        AudioProgress.Progress = 0;
        PlayPauseIcon.Text     = "\uf04b";

        LblCurrentTime.TextColor = Colors.Black;
        LblTotalTime.TextColor   = Colors.Black;
    }

    private async void OnPlayPauseTapped(object sender, EventArgs e)
    {
        if (!PoiService.AudioEnabled)
        {
            await DisplayAlertAsync("Âm thanh đã tắt", "Vui lòng bật âm thanh trong Cài đặt.", "OK");
            return;
        }

        if (sender is Border btn)
        {
            await btn.ScaleToAsync(0.88, 70, Easing.CubicOut);
            await btn.ScaleToAsync(1.0, 120, Easing.SpringOut);
        }

        if (_isSpeaking)
        {
            _ttsService.CancelPlayback();
            _isSpeaking = false;
            PlayPauseIcon.Text = "\uf04b"; // Play icon
            
            LblCurrentTime.TextColor = Colors.Black;
            LblTotalTime.TextColor   = Colors.Black;
        }
        else
        {
            // Stop background audio if playing (fallback just in case)
            if (_playback.IsPlaying)
            {
                await _playback.StopAsync();
            }

            _isSpeaking = true;
            PlayPauseIcon.Text = "\uf04c"; // Pause icon (to signify it's active)
            
            LblCurrentTime.TextColor = Color.FromArgb("#22C55E");
            LblTotalTime.TextColor   = Color.FromArgb("#22C55E");

            try
            {
                await _ttsService.SpeakAsync(LblTranscript.Text, "vi-VN");
            }
            finally
            {
                _isSpeaking = false;
                PlayPauseIcon.Text = "\uf04b"; // Play icon
                LblCurrentTime.TextColor = Colors.Black;
                LblTotalTime.TextColor   = Colors.Black;
            }
        }
    }

    private void OnSpeedTapped(object sender, EventArgs e)
    {
        _speedIndex = (_speedIndex + 1) % _speeds.Length;
        double spd = _speeds[_speedIndex];

        LblSpeed.Text = spd == 1.0 ? "1.0x"
                       : spd == 0.75 ? "0.75x"
                       : spd == 1.25 ? "1.25x"
                       : spd == 1.5  ? "1.5x"
                       : "2.0x";

        _playback.SetSpeed(spd);
    }

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

            var info = new VerticalStackLayout { Spacing = 3, VerticalOptions = LayoutOptions.Center };
            info.Children.Add(new Label { Text = poi.Name, FontFamily = "OpenSans-Semibold", FontSize = 14, TextColor = Color.FromArgb("#0F172A"), LineBreakMode = LineBreakMode.TailTruncation });
            info.Children.Add(new Label { Text = poi.Category, FontFamily = "OpenSans-Regular", FontSize = 12, TextColor = Color.FromArgb("#0F172A") });
            Grid.SetColumn(info, 1);
            grid.Children.Add(info);

            var playIcon = new Label { Text = "\uf04b", FontFamily = "FA6Solid", FontSize = 14, TextColor = Colors.Black, VerticalOptions = LayoutOptions.Center };
            if (_currentPoi != null && _currentPoi.Id == poi.Id)
            {
                card.BackgroundColor = Color.FromArgb("#f1f1f1ff");
                playIcon.Text = "\uf028";
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

    private async void OnTrackItemTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not PoiModel poi) return;

        await _playback.StopAsync();

        LoadPoi(poi);
        BuildTrackList();
    }

    private void OnPlaybackStateChanged(PlaybackState state)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            switch (state)
            {
                case PlaybackState.Playing:
                    PlayPauseIcon.Text = "\uf04c";
                    LblCurrentTime.TextColor = Color.FromArgb("#22C55E");
                    LblTotalTime.TextColor   = Color.FromArgb("#22C55E");
                    break;

                case PlaybackState.Paused:
                    PlayPauseIcon.Text = "\uf04b";
                    LblCurrentTime.TextColor = Color.FromArgb("#EF4444");
                    LblTotalTime.TextColor   = Color.FromArgb("#EF4444");
                    break;

                case PlaybackState.Stopped:
                    PlayPauseIcon.Text = "\uf04b";
                    AudioProgress.Progress = 0;
                    LblCurrentTime.Text = "0:00";
                    LblCurrentTime.TextColor = Colors.Black;
                    LblTotalTime.TextColor   = Colors.Black;
                    break;
            }
        });
    }

    private void OnPlaybackProgressChanged(double position, double duration)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            AudioProgress.Progress = duration > 0 ? position / duration : 0;
            LblCurrentTime.Text    = AudioPlaybackService.FormatTime(position);
            LblTotalTime.Text      = AudioPlaybackService.FormatTime(duration);
        });
    }

    private void OnPlaybackEnded()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            AudioProgress.Progress = 0;
            LblCurrentTime.Text    = "0:00";
            PlayPauseIcon.Text     = "\uf04b";
            LblCurrentTime.TextColor = Colors.Black;
            LblTotalTime.TextColor   = Colors.Black;
        });
    }

    private async void OnPlaybackError(string message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            PlayPauseIcon.Text = "\uf04b";
            await DisplayAlert("Lỗi phát audio", message, "OK");
        });
    }

    private void OnAudioEnabledChanged(bool enabled)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (!enabled)
            {
                await _playback.StopAsync();
                PlayPauseIcon.Text = "\uf04b";
            }
        });
    }

    private void UpdatePlaybackUI()
    {
        if (_playback.IsPlaying)
        {
            PlayPauseIcon.Text = "\uf04c";
            LblCurrentTime.TextColor = Color.FromArgb("#22C55E");
            LblTotalTime.TextColor   = Color.FromArgb("#22C55E");
        }
        else if (_playback.IsPaused)
        {
            PlayPauseIcon.Text = "\uf04b";
            LblCurrentTime.TextColor = Color.FromArgb("#EF4444");
            LblTotalTime.TextColor   = Color.FromArgb("#EF4444");
        }
        else
        {
            PlayPauseIcon.Text = "\uf04b";
            LblCurrentTime.TextColor = Colors.Black;
            LblTotalTime.TextColor   = Colors.Black;
        }
    }

    private string GetAudioFileName()
    {
        if (_currentPoi is null) return string.Empty;
        return $"audio_{_currentPoi.Id}.mp3";
    }
}
