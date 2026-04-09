using AudioTravelling.Mobile.Features.Audio.Services;
using AudioTravelling.Mobile.Features.Audio.Views;
using AudioTravelling.Mobile.Features.Notification.Services;
using AudioTravelling.Mobile.Features.Notification.Views;
using AudioTravelling.Mobile.Features.Order.Models;
using AudioTravelling.Mobile.Features.Order.Services;
using AudioTravelling.Mobile.Features.Order.Views;
using AudioTravelling.Mobile.Features.Poi.Models;
using AudioTravelling.Mobile.Features.Poi.Services;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Controls.Shapes;

namespace AudioTravelling.Mobile.Features.Map.Views;

public partial class MainPage : ContentPage
{
    // ── Dotonbori center ───────────────────────────────────────────────
    private static readonly Location DotonboriCenter = new(34.6687, 135.5013);
    private const double DefaultZoomKm = 0.8;

    private PoiModel? _selectedPoi;
    private bool _isExpanded = false;
    private double _sheetTranslation = 0;

    private readonly AudioPlaybackService _playback = AudioPlaybackService.Instance;
    private readonly NotificationService  _notifSvc  = NotificationService.Instance;
    private readonly CartService          _cartSvc   = CartService.Instance;

    // ── Ctor ──────────────────────────────────────────────────────────
    public MainPage()
    {
        InitializeComponent();

        var span = MapSpan.FromCenterAndRadius(DotonboriCenter, Distance.FromKilometers(DefaultZoomKm));
        DotonboriMap.MoveToRegion(span);

        LoadPoiCards();
        AddMapPins();

        _playback.StateChanged    += OnPlaybackStateChanged;
        _playback.ProgressChanged += OnPlaybackProgressChanged;
        _playback.PlaybackEnded   += OnPlaybackEnded;

        _cartSvc.CartChanged += OnCartChanged;
        _notifSvc.UnreadCountChanged += OnUnreadCountChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = CheckGps();
        UpdateCartBadge();
        UpdateNotificationBadge();
        SyncAudioTab();
    }

    // ══════════════════════════════════════════════════════════════════
    //  GPS
    // ══════════════════════════════════════════════════════════════════
    private async Task CheckGps()
    {
        try
        {
            var loc = await Geolocation.GetLastKnownLocationAsync();
            GpsBanner.IsVisible = loc is null;
        }
        catch
        {
            GpsBanner.IsVisible = true;
        }
    }

    private async void OnOpenGpsSettings(object sender, EventArgs e)
    {
#if ANDROID
        try { AppInfo.ShowSettingsUI(); } catch { }
#endif
        await CheckGps();
    }

    private async void OnMyLocationTapped(object sender, EventArgs e)
    {
        try
        {
            var loc = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5)));
            if (loc is not null)
            {
                DotonboriMap.MoveToRegion(MapSpan.FromCenterAndRadius(loc, Distance.FromKilometers(0.5)));
                GpsBanner.IsVisible = false;
            }
        }
        catch
        {
            GpsBanner.IsVisible = true;
        }
    }

    // ══════════════════════════════════════════════════════════════════
    //  POI CARDS
    // ══════════════════════════════════════════════════════════════════
    private void LoadPoiCards()
    {
        var pois = PoiService.GetSortedByDistance();
        PoiCardStack.Children.Clear();
        EmptyPoiState.IsVisible = pois.Count == 0;

        foreach (var poi in pois)
        {
            var card = BuildPoiCard(poi);
            PoiCardStack.Children.Add(card);
        }
    }

    private View BuildPoiCard(PoiModel poi)
    {
        var outer = new Border
        {
            StrokeShape     = new RoundRectangle { CornerRadius = 18 },
            Stroke          = new SolidColorBrush(Color.FromArgb("#E2E8F0")),
            StrokeThickness = 1,
            BackgroundColor = Colors.White,
            WidthRequest    = 210,
            Padding         = new Thickness(0),
            Shadow          = new Shadow { Brush = Brush.Black, Offset = new Point(0, 4), Opacity = 0.08f, Radius = 12 }
        };

        var stack = new VerticalStackLayout { Spacing = 0 };

        var imgBorder = new Border
        {
            StrokeShape    = new RoundRectangle { CornerRadius = new CornerRadius(18, 18, 0, 0) },
            Stroke         = Brush.Transparent,
            HeightRequest  = 110,
            Content        = new Image { Source = poi.ImageSource, Aspect = Aspect.AspectFill }
        };
        stack.Children.Add(imgBorder);

        var infoStack = new VerticalStackLayout { Padding = new Thickness(12, 10, 12, 12), Spacing = 4 };
        infoStack.Children.Add(new Label { Text = poi.Name, FontFamily = "OpenSans-Semibold", FontSize = 14, TextColor = Color.FromArgb("#0F172A"), LineBreakMode = LineBreakMode.TailTruncation });

        var metaStack = new HorizontalStackLayout { Spacing = 6 };
        metaStack.Children.Add(new Label { Text = poi.RatingLabel, FontFamily = "OpenSans-Regular", FontSize = 11, TextColor = Color.FromArgb("#FBBF24") });
        metaStack.Children.Add(new Label { Text = "•", FontFamily = "OpenSans-Regular", FontSize = 11, TextColor = Color.FromArgb("#CBD5E1") });
        metaStack.Children.Add(new Label { Text = poi.DistanceLabel, FontFamily = "OpenSans-Regular", FontSize = 11, TextColor = Color.FromArgb("#0F172A") });
        metaStack.Children.Add(new Label { Text = "•", FontFamily = "OpenSans-Regular", FontSize = 11, TextColor = Color.FromArgb("#CBD5E1") });
        metaStack.Children.Add(new Label { Text = poi.StatusLabel, FontFamily = "OpenSans-Regular", FontSize = 11, TextColor = poi.StatusColor });
        infoStack.Children.Add(metaStack);
        stack.Children.Add(infoStack);

        outer.Content = stack;

        var tap = new TapGestureRecognizer();
        tap.CommandParameter = poi;
        tap.Tapped += OnPoiCardTapped;
        outer.GestureRecognizers.Add(tap);

        return outer;
    }

    private void OnPoiCardTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not PoiModel poi) return;
        ShowPoiDetail(poi);
    }

    // ══════════════════════════════════════════════════════════════════
    //  MAP PINS
    // ══════════════════════════════════════════════════════════════════
    private void AddMapPins()
    {
        foreach (var poi in PoiService.GetSortedByDistance())
        {
            var offset = poi.Id * 0.0003;
            var pin = new Pin
            {
                Label    = poi.Name,
                Address  = poi.Category,
                Type     = PinType.Place,
                Location = new Location(DotonboriCenter.Latitude + offset, DotonboriCenter.Longitude + offset)
            };
            pin.InfoWindowClicked += (s, _) => ShowPoiDetail(poi);
            DotonboriMap.Pins.Add(pin);
        }
    }

    // ══════════════════════════════════════════════════════════════════
    //  POI DETAIL (expanded sheet)
    // ══════════════════════════════════════════════════════════════════
    private void ShowPoiDetail(PoiModel poi)
    {
        _selectedPoi = poi;
        LblDetailPoiName.Text    = poi.Name;
        DetailHeroImage.Source    = poi.ImageSource;
        DetailRating.Text        = poi.RatingLabel;
        DetailDistance.Text       = poi.DistanceLabel;
        DetailStatus.Text        = poi.StatusLabel;
        DetailDescription.Text   = poi.Description;
        DetailCategory.Text      = poi.Category;

        if (poi.IsOpen)
        {
            ClosedNotice.IsVisible = false;
            MenuLabel.IsVisible    = true;
            MenuList.IsVisible     = true;
            MenuList.ItemsSource   = poi.Menu;
        }
        else
        {
            ClosedNotice.IsVisible = true;
            MenuLabel.IsVisible    = false;
            MenuList.IsVisible     = false;
        }

        ExpandSheet();
    }

    // ══════════════════════════════════════════════════════════════════
    //  SHEET EXPAND / COLLAPSE
    // ══════════════════════════════════════════════════════════════════
    private async void ExpandSheet()
    {
        _isExpanded = true;
        CollapsedContent.IsVisible = false;
        ExpandedContent.IsVisible  = true;
        BottomSheet.MaximumHeightRequest = 9999;
        await ExpandedContent.FadeTo(1, 200);
    }

    private async void CollapseSheet()
    {
        _isExpanded = false;
        await ExpandedContent.FadeTo(0, 150);
        ExpandedContent.IsVisible  = false;
        CollapsedContent.IsVisible = true;
        BottomSheet.MaximumHeightRequest = 680;
    }

    private void OnBackTapped(object sender, EventArgs e)
    {
        CollapseSheet();
    }

    private void OnBottomSheetPan(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                _sheetTranslation = e.TotalY;
                break;
            case GestureStatus.Completed:
                if (_sheetTranslation > 60 && _isExpanded)
                    CollapseSheet();
                else if (_sheetTranslation < -60 && !_isExpanded)
                {
                    var pois = PoiService.GetSortedByDistance();
                    if (pois.Count > 0) ShowPoiDetail(pois[0]);
                }
                _sheetTranslation = 0;
                break;
        }
    }

    private void OnBottomSheetTap(object sender, EventArgs e)
    {
        if (!_isExpanded)
        {
            var pois = PoiService.GetSortedByDistance();
            if (pois.Count > 0) ShowPoiDetail(pois[0]);
        }
    }

    // ══════════════════════════════════════════════════════════════════
    //  TABS (Nearby / Now Playing)
    // ══════════════════════════════════════════════════════════════════
    private bool _tabNearby = true;

    private void OnTabNearbyTapped(object sender, EventArgs e)
    {
        if (_tabNearby) return;
        _tabNearby = true;
        TabNearby.BackgroundColor  = Colors.Black;
        LblTabNearby.TextColor     = Colors.White;
        LblTabNearby.FontFamily    = "OpenSans-Semibold";
        TabPlaying.BackgroundColor = Colors.White;
        LblTabPlaying.TextColor    = Color.FromArgb("#94A3B8");
        LblTabPlaying.FontFamily   = "OpenSans-Regular";

        NearbyScroll.IsVisible     = true;
        MiniPlayerPanel.IsVisible  = false;
    }

    private void OnTabPlayingTapped(object sender, EventArgs e)
    {
        if (!_tabNearby) return;
        _tabNearby = false;
        TabPlaying.BackgroundColor = Colors.Black;
        LblTabPlaying.TextColor    = Colors.White;
        LblTabPlaying.FontFamily   = "OpenSans-Semibold";
        TabNearby.BackgroundColor  = Colors.White;
        LblTabNearby.TextColor     = Color.FromArgb("#94A3B8");
        LblTabNearby.FontFamily    = "OpenSans-Regular";

        NearbyScroll.IsVisible     = false;
        MiniPlayerPanel.IsVisible  = true;
    }

    private void SyncAudioTab()
    {
        if (_playback.IsPlaying || _playback.IsPaused)
        {
            PlayPauseIcon.Text = _playback.IsPlaying ? "\uf04c" : "\uf04b";
            LblAudioStatus.Text = _playback.IsPlaying ? "Đang phát" : "Tạm dừng";
            AudioDot.Color = _playback.IsPlaying ? Color.FromArgb("#10B981") : Color.FromArgb("#EF4444");
            LblAudioStatus.TextColor = _playback.IsPlaying ? Color.FromArgb("#10B981") : Color.FromArgb("#EF4444");
        }
    }

    // ══════════════════════════════════════════════════════════════════
    //  MINI PLAYER
    // ══════════════════════════════════════════════════════════════════
    private async void OnPlayPauseTapped(object sender, EventArgs e)
    {
        if (!PoiService.AudioEnabled)
        {
            await DisplayAlertAsync("Âm thanh đã tắt", "Vui lòng bật âm thanh trong Cài đặt.", "OK");
            return;
        }
        await _playback.TogglePlayPauseAsync("audio_1.mp3");
    }

    private void OnPlaybackStateChanged(PlaybackState state)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            switch (state)
            {
                case PlaybackState.Playing:
                    PlayPauseIcon.Text       = "\uf04c";
                    LblAudioStatus.Text      = "Đang phát";
                    LblAudioStatus.TextColor = Color.FromArgb("#10B981");
                    AudioDot.Color           = Color.FromArgb("#10B981");
                    break;
                case PlaybackState.Paused:
                    PlayPauseIcon.Text       = "\uf04b";
                    LblAudioStatus.Text      = "Tạm dừng";
                    LblAudioStatus.TextColor = Color.FromArgb("#EF4444");
                    AudioDot.Color           = Color.FromArgb("#EF4444");
                    break;
                case PlaybackState.Stopped:
                    PlayPauseIcon.Text       = "\uf04b";
                    LblAudioStatus.Text      = "Đã dừng";
                    LblAudioStatus.TextColor = Color.FromArgb("#94A3B8");
                    AudioDot.Color           = Color.FromArgb("#94A3B8");
                    AudioProgressBar.Progress = 0;
                    break;
            }
        });
    }

    private void OnPlaybackProgressChanged(double position, double duration)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            AudioProgressBar.Progress = duration > 0 ? position / duration : 0;
            LblAudioStatus.Text       = $"Đang phát • {AudioPlaybackService.FormatTime(position)} / {AudioPlaybackService.FormatTime(duration)}";
        });
    }

    private void OnPlaybackEnded()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            PlayPauseIcon.Text        = "\uf04b";
            AudioProgressBar.Progress = 0;
            LblAudioStatus.Text       = "Đã phát xong";
            LblAudioStatus.TextColor  = Color.FromArgb("#94A3B8");
            AudioDot.Color            = Color.FromArgb("#94A3B8");
        });
    }

    private async void OnQueueTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("AudioPlayerPage");
    }

    // ══════════════════════════════════════════════════════════════════
    //  CART
    // ══════════════════════════════════════════════════════════════════
    private async void OnAddToCartTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not FoodItem food) return;
        if (_selectedPoi is null) return;

        if (!_selectedPoi.IsOpen)
        {
            await DisplayAlert("Cửa hàng đóng cửa", "Cửa hàng này hiện đang đóng cửa, vui lòng quay lại sau.", "OK");
            return;
        }

        _cartSvc.Add(_selectedPoi, food);

        if (sender is Border btn)
        {
            await btn.ScaleToAsync(0.85, 60, Easing.CubicOut);
            await btn.ScaleToAsync(1.0, 100, Easing.SpringOut);
        }
    }

    private async void OnCartTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("OrderPage");
    }

    private void UpdateCartBadge()
    {
        int count = _cartSvc.TotalCount;
        CartBadge.IsVisible   = count > 0;
        CartItemCount.Text    = count.ToString();
    }

    private void OnCartChanged()
    {
        MainThread.BeginInvokeOnMainThread(UpdateCartBadge);
    }

    // ══════════════════════════════════════════════════════════════════
    //  NOTIFICATIONS
    // ══════════════════════════════════════════════════════════════════
    private async void OnNotificationsTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("NotificationsPage");
    }

    private void UpdateNotificationBadge()
    {
        int count = _notifSvc.UnreadCount;
        NotificationBadge.IsVisible = count > 0;
        NotificationCount.Text      = count.ToString();
    }

    private void OnUnreadCountChanged()
    {
        MainThread.BeginInvokeOnMainThread(UpdateNotificationBadge);
    }

    // ══════════════════════════════════════════════════════════════════
    //  SEARCH
    // ══════════════════════════════════════════════════════════════════
    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        string query = e.NewTextValue?.Trim().ToLowerInvariant() ?? "";

        var pois = string.IsNullOrEmpty(query)
            ? PoiService.GetSortedByDistance()
            : PoiService.GetSortedByDistance().Where(p =>
                  p.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                  p.Category.Contains(query, StringComparison.CurrentCultureIgnoreCase)).ToList();

        PoiCardStack.Children.Clear();
        EmptyPoiState.IsVisible = pois.Count == 0;

        foreach (var poi in pois)
            PoiCardStack.Children.Add(BuildPoiCard(poi));
    }

    private new async Task DisplayAlertAsync(string title, string message, string cancel)
    {
        await DisplayAlert(title, message, cancel);
    }
}
