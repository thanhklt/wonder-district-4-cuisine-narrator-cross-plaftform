using AudioTravelling.Mobile.Features.Audio.Services;
using AudioTravelling.Mobile.Features.Notification.Services;
using AudioTravelling.Mobile.Features.Order.Models;
using AudioTravelling.Mobile.Features.Order.Services;
using AudioTravelling.Mobile.Features.Poi.Models;
using AudioTravelling.Mobile.Services.Api;
using AudioTravelling.Mobile.Services.Api.Interfaces;
using AudioTravelling.Mobile.Services.Api.Responses;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Maps;

namespace AudioTravelling.Mobile.Features.Map.Views;

public partial class MainPage : ContentPage
{
    private static readonly Location DotonboriCenter = new(10.7570, 106.7075);
    private const double DefaultZoomKm = 0.8;

    private readonly IPoiApiService _poiApiService;

    private PoiModel? _selectedPoi;
    private bool _isExpanded = false;
    private double _sheetTranslation = 0;
    private bool _isPoiLoaded = false;

    private List<PoiModel> _pois = new();

    private readonly AudioPlaybackService _playback = AudioPlaybackService.Instance;
    private readonly NotificationService _notifSvc = NotificationService.Instance;
    private readonly CartService _cartSvc = CartService.Instance;

    public MainPage(IPoiApiService poiApiService)
    {
        InitializeComponent();

        _poiApiService = poiApiService ?? throw new ArgumentNullException(nameof(poiApiService));

        var span = MapSpan.FromCenterAndRadius(DotonboriCenter, Distance.FromKilometers(DefaultZoomKm));
        DotonboriMap.MoveToRegion(span);

        _playback.StateChanged += OnPlaybackStateChanged;
        _playback.ProgressChanged += OnPlaybackProgressChanged;
        _playback.PlaybackEnded += OnPlaybackEnded;

        _cartSvc.CartChanged += OnCartChanged;
        //_notifSvc.UnreadCountChanged += OnUnreadCountChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _ = CheckGps();
        UpdateCartBadge();
        //UpdateNotificationBadge();
        SyncAudioTab();

        if (!_isPoiLoaded)
        {
            await LoadPoisFromApiAsync();
            _isPoiLoaded = true;
        }
    }

    private async Task LoadPoisFromApiAsync()
    {
        try
        {
            var items = await _poiApiService.GetAllAsync();

            if (items is null)
            {
                System.Diagnostics.Debug.WriteLine("[LoadPoisFromApiAsync] API trả về rỗng.");

                _pois = new List<PoiModel>();
                LoadPoiCards(_pois);
                AddMapPins(_pois);
                return;
            }

            foreach (var item in items)
            {
                System.Diagnostics.Debug.WriteLine($"POI ID: {item.PoiId}");
                System.Diagnostics.Debug.WriteLine($"POI Name: {item.Name}");
                System.Diagnostics.Debug.WriteLine($"RAW COVER: {item.CoverImageUrl}");
                System.Diagnostics.Debug.WriteLine($"ABS COVER: {ApiUrlHelper.ToAbsoluteUrl(item.CoverImageUrl)}");
            }

            _pois = items
                .Where(x => x != null)
                .Select(MapToPoiModel)
                .ToList();

            foreach (var poi in _pois)
            {
                System.Diagnostics.Debug.WriteLine($"POI: {poi.NameVi}");
                System.Diagnostics.Debug.WriteLine($"CoverImageUrl: {poi.CoverImageUrl}");

                if (poi.Images == null || poi.Images.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Mapped Images: <empty>");
                }
                else
                {
                    foreach (var img in poi.Images)
                    {
                        System.Diagnostics.Debug.WriteLine($"Mapped IMG: {img}");
                    }
                }
            }

            LoadPoiCards(_pois);
            AddMapPins(_pois);

            var firstValidPoi = _pois.FirstOrDefault(HasValidCoordinate);
            if (firstValidPoi != null)
            {
                DotonboriMap.MoveToRegion(
                    MapSpan.FromCenterAndRadius(
                        new Location(firstValidPoi.Latitude, firstValidPoi.Longitude),
                        Distance.FromKilometers(DefaultZoomKm)));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MainPage.LoadPoisFromApiAsync] {ex}");
            await DisplayAlert("Lỗi", "Không tải được dữ liệu địa điểm.", "OK");

            _pois = new List<PoiModel>();
            LoadPoiCards(_pois);
            AddMapPins(_pois);
        }
    }

    private static PoiModel MapToPoiModel(PoiResponse item)
    {
        var coverImageUrl = ApiUrlHelper.ToAbsoluteUrl(item.CoverImageUrl);

        System.Diagnostics.Debug.WriteLine($"[MapToPoiModel] PoiId={item.PoiId}");
        System.Diagnostics.Debug.WriteLine($"[MapToPoiModel] API Cover={item.CoverImageUrl}");
        System.Diagnostics.Debug.WriteLine($"[MapToPoiModel] Final Cover={coverImageUrl}");

        return new PoiModel
        {
            PoiId = item.PoiId,
            NameVi = item.Name ?? string.Empty,
            DescriptionVi = item.DescriptionVi ?? "Chưa có mô tả cho địa điểm này.",
            Latitude = item.Latitude,
            Longitude = item.Longitude,
            Radius = item.Radius,
            Priority = item.Priority,
            IsActive = true,
            CoverImageUrl = coverImageUrl,
            Images = string.IsNullOrWhiteSpace(coverImageUrl)
                ? new List<string>()
                : new List<string> { coverImageUrl }
        };
    }

    private static bool HasValidCoordinate(PoiModel poi)
    {
        return poi.Latitude >= -90 && poi.Latitude <= 90
            && poi.Longitude >= -180 && poi.Longitude <= 180;
    }

    private static int GetValidRadius(PoiModel poi)
    {
        return poi.Radius > 0 ? poi.Radius : 30;
    }

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
            var loc = await Geolocation.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5)));

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

    private void LoadPoiCards(List<PoiModel> pois)
    {
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
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Stroke = new SolidColorBrush(Color.FromArgb("#E27D60")),
            StrokeThickness = 1,
            BackgroundColor = Colors.White,
            WidthRequest = 210,
            Padding = new Thickness(0),
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Offset = new Point(0, 4),
                Opacity = 0.08f,
                Radius = 12
            }
        };

        var stack = new VerticalStackLayout
        {
            Spacing = 0
        };

        var imageSource = string.IsNullOrWhiteSpace(poi.CoverImageUrl)
            ? "placeholder_poi.png"
            : ImageSource.FromUri(new Uri(poi.CoverImageUrl));

        var imageBorder = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(18, 18, 0, 0) },
            Stroke = Brush.Transparent,
            HeightRequest = 120,
            BackgroundColor = Color.FromArgb("#E2E8F0"),
            Content = new Image
            {
                Source = imageSource,
                Aspect = Aspect.AspectFill
            }
        };

        var infoStack = new VerticalStackLayout
        {
            Padding = new Thickness(12, 12, 12, 12),
            Spacing = 6
        };

        infoStack.Children.Add(new Label
        {
            Text = poi.NameVi,
            FontFamily = "OpenSans-Semibold",
            FontSize = 14,
            TextColor = Color.FromArgb("#0F172A"),
            LineBreakMode = LineBreakMode.TailTruncation
        });

        infoStack.Children.Add(new Label
        {
            Text = poi.IsActive ? "Đang hoạt động" : "Ngừng hoạt động",
            FontFamily = "OpenSans-Regular",
            FontSize = 11,
            TextColor = poi.IsActive ? Color.FromArgb("#10B981") : Color.FromArgb("#EF4444")
        });

        stack.Children.Add(imageBorder);
        stack.Children.Add(infoStack);

        outer.Content = stack;

        var tap = new TapGestureRecognizer();
        tap.CommandParameter = poi;
        tap.Tapped += OnPoiCardTapped;
        outer.GestureRecognizers.Add(tap);

        return outer;
    }

    private async void OnPoiCardTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not PoiModel poi) return;
        await ShowPoiDetailAsync(poi);
    }

    private void AddMapPins(List<PoiModel> pois)
    {
        DotonboriMap.Pins.Clear();
        DotonboriMap.MapElements.Clear();

        foreach (var poi in pois.Where(HasValidCoordinate))
        {
            var location = new Location(poi.Latitude, poi.Longitude);

            var pin = new Pin
            {
                Label = poi.NameVi,
                Address = poi.DescriptionVi,
                Type = PinType.Place,
                Location = location
            };

            pin.InfoWindowClicked += async (s, _) => await ShowPoiDetailAsync(poi);
            DotonboriMap.Pins.Add(pin);

            var circle = new Circle
            {
                Center = location,
                Radius = Distance.FromMeters(GetValidRadius(poi)),
                StrokeColor = poi.IsActive ? Color.FromArgb("#E27D60") : Color.FromArgb("#94A3B8"),
                StrokeWidth = 2,
                FillColor = poi.IsActive ? Color.FromArgb("#33E27D60") : Color.FromArgb("#3394A3B8")
            };

            DotonboriMap.MapElements.Add(circle);
        }
    }

    private async Task ShowPoiDetailAsync(PoiModel poi)
    {
        try
        {
            var detail = await _poiApiService.GetByIdAsync(poi.PoiId.ToString());

            if (detail != null)
            {
                poi.NameVi = detail.Name ?? poi.NameVi;
                poi.DescriptionVi = detail.DescriptionVi ?? poi.DescriptionVi;
                poi.Latitude = detail.Latitude;
                poi.Longitude = detail.Longitude;
                poi.Radius = detail.Radius;
                poi.Priority = detail.Priority;

                var detailCover = ApiUrlHelper.ToAbsoluteUrl(detail.CoverImageUrl);
                if (!string.IsNullOrWhiteSpace(detailCover))
                {
                    poi.CoverImageUrl = detailCover;
                    poi.Images = new List<string> { detailCover };
                }

            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ShowPoiDetailAsync] {ex}");
        }

        ShowPoiDetail(poi);
    }

    private void ShowPoiDetail(PoiModel poi)
    {
        _selectedPoi = poi;

        LblDetailPoiName.Text = poi.NameVi;
        DetailDescription.Text = string.IsNullOrWhiteSpace(poi.DescriptionVi)
        ? "Chưa có mô tả cho địa điểm này."
        : poi.DescriptionVi;

        if (DetailHeroImage != null)
        {
            DetailHeroImage.Source = string.IsNullOrWhiteSpace(poi.CoverImageUrl)
                ? "placeholder_poi.png"
                : ImageSource.FromUri(new Uri(poi.CoverImageUrl));
        }

        ClosedNotice.IsVisible = !poi.IsActive;
        MenuLabel.IsVisible = false;
        MenuList.IsVisible = false;
        MenuList.ItemsSource = null;

        ExpandSheet();
    }

    private async void ExpandSheet()
    {
        _isExpanded = true;
        CollapsedContent.IsVisible = false;
        ExpandedContent.IsVisible = true;
        BottomSheet.MaximumHeightRequest = 9999;
        await ExpandedContent.FadeTo(1, 200);
    }

    private async void CollapseSheet()
    {
        _isExpanded = false;
        await ExpandedContent.FadeTo(0, 150);
        ExpandedContent.IsVisible = false;
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
                {
                    CollapseSheet();
                }
                else if (_sheetTranslation < -60 && !_isExpanded)
                {
                    if (_pois.Count > 0)
                        ShowPoiDetail(_pois[0]);
                }

                _sheetTranslation = 0;
                break;
        }
    }

    private void OnBottomSheetTap(object sender, EventArgs e)
    {
        if (!_isExpanded && _pois.Count > 0)
        {
            ShowPoiDetail(_pois[0]);
        }
    }

    private bool _tabNearby = true;

    private void OnTabNearbyTapped(object sender, EventArgs e)
    {
        if (_tabNearby) return;

        _tabNearby = true;
        TabNearby.BackgroundColor = Colors.White;
        LblTabNearby.TextColor = Colors.Black;
        LblTabNearby.FontFamily = "OpenSans-Semibold";
        TabPlaying.BackgroundColor = Color.FromArgb("#E27D60");
        LblTabPlaying.TextColor = Colors.White;
        LblTabPlaying.FontFamily = "OpenSans-Regular";

        NearbyScroll.IsVisible = true;
        MiniPlayerPanel.IsVisible = false;
    }

    private void OnTabPlayingTapped(object sender, EventArgs e)
    {
        if (!_tabNearby) return;

        _tabNearby = false;
        TabPlaying.BackgroundColor = Colors.White;
        LblTabPlaying.TextColor = Colors.Black;
        LblTabPlaying.FontFamily = "OpenSans-Semibold";
        TabNearby.BackgroundColor = Color.FromArgb("#E27D60");
        LblTabNearby.TextColor = Colors.White;
        LblTabNearby.FontFamily = "OpenSans-Regular";

        NearbyScroll.IsVisible = false;
        MiniPlayerPanel.IsVisible = true;
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

    private async void OnPlayPauseTapped(object sender, EventArgs e)
    {
        if (!AudioTravelling.Mobile.Features.Poi.Services.PoiService.AudioEnabled)
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
                    PlayPauseIcon.Text = "\uf04c";
                    LblAudioStatus.Text = "Đang phát";
                    LblAudioStatus.TextColor = Color.FromArgb("#10B981");
                    AudioDot.Color = Color.FromArgb("#10B981");
                    break;

                case PlaybackState.Paused:
                    PlayPauseIcon.Text = "\uf04b";
                    LblAudioStatus.Text = "Tạm dừng";
                    LblAudioStatus.TextColor = Color.FromArgb("#EF4444");
                    AudioDot.Color = Color.FromArgb("#EF4444");
                    break;

                case PlaybackState.Stopped:
                    PlayPauseIcon.Text = "\uf04b";
                    LblAudioStatus.Text = "Đã dừng";
                    LblAudioStatus.TextColor = Color.FromArgb("#94A3B8");
                    AudioDot.Color = Color.FromArgb("#94A3B8");
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
            LblAudioStatus.Text = $"Đang phát • {AudioPlaybackService.FormatTime(position)} / {AudioPlaybackService.FormatTime(duration)}";
        });
    }

    private void OnPlaybackEnded()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            PlayPauseIcon.Text = "\uf04b";
            AudioProgressBar.Progress = 0;
            LblAudioStatus.Text = "Đã phát xong";
            LblAudioStatus.TextColor = Color.FromArgb("#94A3B8");
            AudioDot.Color = Color.FromArgb("#94A3B8");
        });
    }

    private async void OnQueueTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("AudioPlayerPage");
    }

    private async void OnAddToCartTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not FoodItem food) return;
        if (_selectedPoi is null) return;

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
        CartBadge.IsVisible = count > 0;
        CartItemCount.Text = count.ToString();
    }

    private void OnCartChanged()
    {
        MainThread.BeginInvokeOnMainThread(UpdateCartBadge);
    }

    /*private async void OnNotificationsTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("NotificationsPage");
    }

    private void UpdateNotificationBadge()
    {
        int count = _notifSvc.UnreadCount;
        NotificationBadge.IsVisible = count > 0;
        NotificationCount.Text = count.ToString();
    }

    private void OnUnreadCountChanged()
    {
        MainThread.BeginInvokeOnMainThread(UpdateNotificationBadge);
    }*/

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        string query = e.NewTextValue?.Trim() ?? "";

        var filtered = string.IsNullOrWhiteSpace(query)
            ? _pois
            : _pois.Where(p =>
                p.NameVi.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                p.DescriptionVi.Contains(query, StringComparison.CurrentCultureIgnoreCase))
            .ToList();

        LoadPoiCards(filtered);
        AddMapPins(filtered);
    }

    private new async Task DisplayAlertAsync(string title, string message, string cancel)
    {
        await DisplayAlert(title, message, cancel);
    }
}