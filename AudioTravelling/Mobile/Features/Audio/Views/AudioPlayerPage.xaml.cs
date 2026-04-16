using AudioTravelling.Mobile.Features.Audio.Services;
using AudioTravelling.Mobile.Features.Poi.Models;
using AudioTravelling.Mobile.Features.Poi.Services;
using AudioTravelling.Mobile.Services.Api.Interfaces;
using AudioTravelling.Mobile.Services.Api.Responses;
using Microsoft.Maui.Controls.Shapes;

namespace AudioTravelling.Mobile.Features.Audio.Views;

public partial class AudioPlayerPage : ContentPage
{
    private bool _trackListOpen = false;
    private int _speedIndex = 1;
    private readonly double[] _speeds = { 0.75, 1.0, 1.25, 1.5, 2.0 };

    private PoiModel? _currentPoi;
    private List<PoiModel> _pois = new();
    private bool _isLoaded = false;

    private readonly AudioPlaybackService _playback = AudioPlaybackService.Instance;
    private readonly ITextToSpeechService _ttsService;
    private readonly IPoiApiService _poiApiService;

    private bool _isSpeaking = false;

    public AudioPlayerPage(ITextToSpeechService ttsService, IPoiApiService poiApiService)
    {
        _ttsService = ttsService ?? throw new ArgumentNullException(nameof(ttsService));
        _poiApiService = poiApiService ?? throw new ArgumentNullException(nameof(poiApiService));

        InitializeComponent();

        PoiService.AudioEnabledChanged += OnAudioEnabledChanged;

        _playback.StateChanged += OnPlaybackStateChanged;
        _playback.ProgressChanged += OnPlaybackProgressChanged;
        _playback.PlaybackEnded += OnPlaybackEnded;
        _playback.ErrorOccurred += OnPlaybackError;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isLoaded)
        {
            await LoadPoisFromApiAsync();
            _isLoaded = true;
        }

        if (!PoiService.AudioEnabled)
        {
            _ = _playback.StopAsync();
            _ttsService.CancelPlayback();
            _isSpeaking = false;
            PlayPauseIcon.Text = "\uf04b";
        }

        UpdatePlaybackUI();
    }

    private async Task LoadPoisFromApiAsync()
    {
        try
        {
            var items = await _poiApiService.GetAllAsync();

            _pois = items?
                .Where(x => x != null)
                .Select(MapToPoiModel)
                .ToList() ?? new List<PoiModel>();

            if (_pois.Count > 0)
            {
                LoadPoi(_pois[0]);
            }
            else
            {
                ResetAudioUI();
            }

            BuildTrackList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AudioPlayerPage.LoadPoisFromApiAsync] {ex}");
            _pois = new List<PoiModel>();
            ResetAudioUI();
            BuildTrackList();
            await DisplayAlert("Lỗi", "Không tải được danh sách audio địa điểm.", "OK");
        }
    }

    private static PoiModel MapToPoiModel(PoiResponse item)
    {
        return new PoiModel
        {
            PoiId = item.PoiId,
            NameVi = item.Name ?? string.Empty,
            DescriptionVi = item.DescriptionVi ?? string.Empty,
            Latitude = item.Latitude,
            Longitude = item.Longitude,
            Radius = item.Radius,
            IsActive = true
        };
    }

    private void ResetAudioUI()
    {
        _currentPoi = null;

        HeroImage.Source = null;
        LblTrackTitle.Text = "Chưa có dữ liệu";
        LblTrackSubtitle.Text = "Không có POI khả dụng";
        LblTranscript.Text = string.Empty;

        LblCurrentTime.Text = "0:00";
        LblTotalTime.Text = "0:00";
        AudioProgress.Progress = 0;
        PlayPauseIcon.Text = "\uf04b";

        LblCurrentTime.TextColor = Colors.Black;
        LblTotalTime.TextColor = Colors.Black;
    }

    private void LoadPoi(PoiModel poi)
    {
        _currentPoi = poi;

        System.Diagnostics.Debug.WriteLine($"[LoadPoi] PoiId={poi.PoiId}, Name={poi.NameVi}");
        System.Diagnostics.Debug.WriteLine($"[LoadPoi] DescriptionVi='{poi.DescriptionVi}'");

        HeroImage.Source = null;
        LblTrackTitle.Text = poi.NameVi;
        LblTrackSubtitle.Text = poi.IsActive ? "POI • Đang hoạt động" : "POI • Ngừng hoạt động";
        LblTranscript.Text = poi.DescriptionVi;

        LblCurrentTime.Text = "0:00";
        LblTotalTime.Text = "0:00";
        AudioProgress.Progress = 0;
        PlayPauseIcon.Text = "\uf04b";

        LblCurrentTime.TextColor = Colors.Black;
        LblTotalTime.TextColor = Colors.Black;
    }

    private async void OnPlayPauseTapped(object sender, EventArgs e)
    {
        if (!PoiService.AudioEnabled)
        {
            await DisplayAlertAsync("Âm thanh đã tắt", "Vui lòng bật âm thanh trong Cài đặt.", "OK");
            return;
        }

        if (_currentPoi == null)
        {
            await DisplayAlertAsync("Chưa có dữ liệu", "Không có nội dung để phát.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(LblTranscript.Text))
        {
            await DisplayAlertAsync("Thiếu nội dung", "POI này chưa có nội dung audio.", "OK");
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
            PlayPauseIcon.Text = "\uf04b";

            LblCurrentTime.TextColor = Colors.Black;
            LblTotalTime.TextColor = Colors.Black;
            return;
        }

        _isSpeaking = true;
        PlayPauseIcon.Text = "\uf04c";

        LblCurrentTime.TextColor = Color.FromArgb("#22C55E");
        LblTotalTime.TextColor = Color.FromArgb("#22C55E");

        try
        {
            await _ttsService.SpeakAsync(LblTranscript.Text);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AudioPlayerPage.OnPlayPauseTapped] {ex}");
            await DisplayAlert("Lỗi", "Không thể phát nội dung audio.", "OK");
        }
        finally
        {
            _isSpeaking = false;
            PlayPauseIcon.Text = "\uf04b";
            LblCurrentTime.TextColor = Colors.Black;
            LblTotalTime.TextColor = Colors.Black;
        }
    }

    private void OnSpeedTapped(object sender, EventArgs e)
    {
        _speedIndex = (_speedIndex + 1) % _speeds.Length;
        double spd = _speeds[_speedIndex];

        LblSpeed.Text = spd == 1.0 ? "1.0x"
            : spd == 0.75 ? "0.75x"
            : spd == 1.25 ? "1.25x"
            : spd == 1.5 ? "1.5x"
            : "2.0x";

        _playback.SetSpeed(spd);
    }

    private void OnTrackListTapped(object sender, EventArgs e)
    {
        _trackListOpen = !_trackListOpen;
        TrackListPanel.IsVisible = _trackListOpen;
        LblTrackListHeader.IsVisible = _trackListOpen;
    }

    private void BuildTrackList()
    {
        TrackListPanel.Children.Clear();

        foreach (var poi in _pois)
        {
            var card = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                Stroke = new SolidColorBrush(Color.FromArgb("#E2E8F0")),
                StrokeThickness = 1,
                BackgroundColor = Colors.White,
                Padding = new Thickness(14),
                Shadow = new Shadow
                {
                    Brush = Brush.Black,
                    Offset = new Point(0, 2),
                    Opacity = 0.05f,
                    Radius = 6
                }
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                ColumnSpacing = 12
            };

            var info = new VerticalStackLayout
            {
                Spacing = 3,
                VerticalOptions = LayoutOptions.Center
            };

            info.Children.Add(new Label
            {
                Text = poi.NameVi,
                FontFamily = "OpenSans-Semibold",
                FontSize = 14,
                TextColor = Color.FromArgb("#0F172A"),
                LineBreakMode = LineBreakMode.TailTruncation
            });

            info.Children.Add(new Label
            {
                Text = poi.DescriptionVi,
                FontFamily = "OpenSans-Regular",
                FontSize = 12,
                TextColor = Color.FromArgb("#64748B"),
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 2
            });

            Grid.SetColumn(info, 0);
            grid.Children.Add(info);

            var playIcon = new Label
            {
                Text = "\uf04b",
                FontFamily = "FA6Solid",
                FontSize = 14,
                TextColor = Colors.Black,
                VerticalOptions = LayoutOptions.Center
            };

            if (_currentPoi != null && _currentPoi.PoiId == poi.PoiId)
            {
                card.BackgroundColor = Color.FromArgb("#F8FAFC");
                playIcon.Text = "\uf028";
                playIcon.TextColor = Color.FromArgb("#E27D60");
            }

            Grid.SetColumn(playIcon, 1);
            grid.Children.Add(playIcon);

            card.Content = grid;

            var tap = new TapGestureRecognizer
            {
                CommandParameter = poi
            };
            tap.Tapped += OnTrackItemTapped;
            card.GestureRecognizers.Add(tap);

            TrackListPanel.Children.Add(card);
        }
    }

    private async void OnTrackItemTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not PoiModel poi) return;

        try
        {
            _ttsService.CancelPlayback();
            _isSpeaking = false;
            await _playback.StopAsync();

            var enrichedPoi = await EnrichPoiAsync(poi);

            LoadPoi(enrichedPoi);
            BuildTrackList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AudioPlayerPage.OnTrackItemTapped] {ex}");
        }
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
                    LblTotalTime.TextColor = Color.FromArgb("#22C55E");
                    break;

                case PlaybackState.Paused:
                    PlayPauseIcon.Text = "\uf04b";
                    LblCurrentTime.TextColor = Color.FromArgb("#EF4444");
                    LblTotalTime.TextColor = Color.FromArgb("#EF4444");
                    break;

                case PlaybackState.Stopped:
                    PlayPauseIcon.Text = "\uf04b";
                    AudioProgress.Progress = 0;
                    LblCurrentTime.Text = "0:00";
                    LblCurrentTime.TextColor = Colors.Black;
                    LblTotalTime.TextColor = Colors.Black;
                    break;
            }
        });
    }

    private void OnPlaybackProgressChanged(double position, double duration)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            AudioProgress.Progress = duration > 0 ? position / duration : 0;
            LblCurrentTime.Text = AudioPlaybackService.FormatTime(position);
            LblTotalTime.Text = AudioPlaybackService.FormatTime(duration);
        });
    }

    private void OnPlaybackEnded()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            AudioProgress.Progress = 0;
            LblCurrentTime.Text = "0:00";
            PlayPauseIcon.Text = "\uf04b";
            LblCurrentTime.TextColor = Colors.Black;
            LblTotalTime.TextColor = Colors.Black;
        });
    }

    private void OnPlaybackError(string message)
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
                _ttsService.CancelPlayback();
                _isSpeaking = false;
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
            LblTotalTime.TextColor = Color.FromArgb("#22C55E");
        }
        else if (_playback.IsPaused)
        {
            PlayPauseIcon.Text = "\uf04b";
            LblCurrentTime.TextColor = Color.FromArgb("#EF4444");
            LblTotalTime.TextColor = Color.FromArgb("#EF4444");
        }
        else
        {
            PlayPauseIcon.Text = "\uf04b";
            LblCurrentTime.TextColor = Colors.Black;
            LblTotalTime.TextColor = Colors.Black;
        }
    }

    private async Task<PoiModel> EnrichPoiAsync(PoiModel poi)
    {
        try
        {
            var detail = await _poiApiService.GetByIdAsync(poi.PoiId.ToString());

            if (detail != null)
            {
                poi.NameVi = detail.Name ?? poi.NameVi;
                poi.DescriptionVi = detail.DescriptionVi ?? poi.DescriptionVi;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AudioPlayerPage.EnrichPoiAsync] {ex}");
        }

        return poi;
    }

    private new async Task DisplayAlertAsync(string title, string message, string cancel)
    {
        await DisplayAlert(title, message, cancel);
    }
}