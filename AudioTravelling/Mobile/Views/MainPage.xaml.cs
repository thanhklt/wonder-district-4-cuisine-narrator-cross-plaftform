using Mobile.Models;
using Mobile.Services;
using Microsoft.Maui.Controls.Shapes;

namespace Mobile.Views;

public partial class MainPage : ContentPage
{
    // ── State ──────────────────────────────────────────────────────────────
    private bool   _isExpanded  = false;
    private bool   _isPlaying   = true;
    private bool   _showNearby  = true;   // true = Gần bạn tab, false = Đang phát tab
    private double _sheetStartY = 0;

    private bool _isPageActive = false;       // BÍ QUYẾT 1: Chặn lỗi kẹt GPS khi chuyển trang nhanh
    private bool _isTrackingLocation = false; // Dùng để theo dõi GPS có được bật hay không
    private Location _lastLocation; // Lưu vị trí cuối cùng để tính khoảng cách di chuyển
    private const  double DRAG_THRESHOLD = 55;

    private PoiModel? _selectedPoi = null;

    // ── Init ───────────────────────────────────────────────────────────────
    public MainPage()
    {
        InitializeComponent();

        // Subscribe to cart changes to update badge
        CartService.Instance.CartChanged += UpdateCartBadge;

        // Subscribe to audio toggle from Settings
        PoiService.AudioEnabledChanged += OnAudioEnabledChanged;

        // Subscribe to notification changes
        NotificationService.Instance.UnreadCountChanged += UpdateNotificationBadge;

        // Build POI cards
        BuildPoiCards();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _isPageActive = true; // Báo cho hệ thống biết trang đang mở

        // Bật lại con trỏ chấm xanh của bản đồ
        if (DotonboriMap != null)
        {
            DotonboriMap.IsShowingUser = true;
        }

        UpdateCartBadge();
        UpdateNotificationBadge();
        UpdateCtaSubtitle();
        await CheckGpsAndConnectivity();

        // Bắt đầu lắng nghe vị trí tiết kiệm pin khi vào trang này
        await StartOptimalLocationTracking();

        // Mock: Notify if close to POI
        var closestPoi = PoiService.GetSortedByDistance().FirstOrDefault(p => p.DistanceMeters <= 50);
        if (closestPoi != null)
        {
            NotificationService.Instance.AddArrivingAtPoiNotification(closestPoi);
        }
    }

    // THÊM HÀM NÀY ĐỂ TẮT GPS KHI ẨN APP
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _isPageActive = false; // Báo cho hệ thống biết trang đã đóng

        StopOptimalLocationTracking();

        // QUAN TRỌNG: Tắt luôn con trỏ chấm xanh để ngắt hẳn GPS của Google Maps
        if (DotonboriMap != null)
        {
            DotonboriMap.IsShowingUser = false;
        }
    }
    private void OnLocationChanged(object sender, GeolocationLocationChangedEventArgs e)
    {
        var newLocation = e.Location;

        // BÍ QUYẾT 2: Tự động kiểm tra khoảng cách (15 mét) để tránh máy chạy tính toán dư thừa
        if (_lastLocation != null)
        {
            double distanceInMeters = Location.CalculateDistance(_lastLocation, newLocation, DistanceUnits.Kilometers) * 1000;
            if (distanceInMeters < 5)
            {
                return; // Đứng im hoặc di chuyển chưa đủ 5 mét -> Dừng, không tính toán tiếp
            }
        }
        _lastLocation = newLocation;

        // 1. Cập nhật vị trí lên bản đồ (nếu cần xử lý thủ công)
        // DotonboriMap.MoveToRegion(MapSpan.FromCenterAndRadius(newLocation, Distance.FromKilometers(1)));

        // 2. Logic kiểm tra xem có gần POI nào không
        var closestPoi = PoiService.GetSortedByDistance().FirstOrDefault(p => p.DistanceMeters <= 50);
        if (closestPoi != null)
        {
            NotificationService.Instance.AddArrivingAtPoiNotification(closestPoi);
        }
    }
    private async Task StartOptimalLocationTracking()
    {
        if (_isTrackingLocation) return;

        try
        {
            var request = new GeolocationListeningRequest
            {
                DesiredAccuracy = GeolocationAccuracy.High,
                MinimumTime = TimeSpan.FromSeconds(10) // Ngủ ít nhất 10 giây giữa các lần quét
            };

            Geolocation.LocationChanged += OnLocationChanged;

            bool success = await Geolocation.StartListeningForegroundAsync(request);

            // XỬ LÝ LỖI KẸT GPS: Nếu bật xong mà người dùng đã qua trang khác thì ép tắt ngay!
            if (!_isPageActive)
            {
                Geolocation.StopListeningForeground();
                Geolocation.LocationChanged -= OnLocationChanged;
                _isTrackingLocation = false;
                return;
            }

            if (success)
            {
                _isTrackingLocation = true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi bật tracking: {ex.Message}");
        }
    }

    private void StopOptimalLocationTracking()
    {
        // Ép buộc tắt lắng nghe bất chấp trạng thái để an toàn 100%
        try
        {
            Geolocation.LocationChanged -= OnLocationChanged;
            Geolocation.StopListeningForeground();
        }
        catch { /* Bỏ qua nếu chưa bật kịp */ }
        finally
        {
            _isTrackingLocation = false;
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  GPS & CONNECTIVITY CHECKS (EC-H1, EC-H4, BR-017)
    // ══════════════════════════════════════════════════════════════════════

    private async Task CheckGpsAndConnectivity()
    {
        try
        {
            var location = await Geolocation.GetLastKnownLocationAsync();
            if (location == null)
            {
                // Try to get current location
                location = await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(5)
                });
            }

            GpsBanner.IsVisible = (location == null);
        }
        catch (FeatureNotSupportedException)
        {
            GpsBanner.IsVisible = true;
        }
        catch (FeatureNotEnabledException)
        {
            // GPS is disabled
            GpsBanner.IsVisible = true;
        }
        catch (PermissionException)
        {
            GpsBanner.IsVisible = true;
        }
        catch (Exception)
        {
            // Fallback: hide banner, show data anyway
            GpsBanner.IsVisible = false;
        }

        // Check connectivity (EC-H4)
        var connectivity = Connectivity.NetworkAccess;
        if (connectivity != NetworkAccess.Internet)
        {
            // Show cached data (already loaded from PoiService)
            // but show a snackbar warning
            await DisplayAlertAsync("Không có kết nối",
                "Không thể tải dữ liệu mới. Đang hiển thị dữ liệu đã lưu.", "OK");
        }

        // Check if POI list is empty (EC-H3)
        var pois = PoiService.GetSortedByDistance();
        EmptyPoiState.IsVisible = pois.Count == 0;
        BottomSheet.IsVisible   = pois.Count > 0;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  POI NEARBY LIST  (horizontal scroll cards, built in code-behind)
    // ══════════════════════════════════════════════════════════════════════

    private void BuildPoiCards()
    {
        var pois = PoiService.GetSortedByDistance();
        PoiCardStack.Children.Clear();

        foreach (var poi in pois)
        {
            // Card root
            var card = new Border
            {
                StrokeShape    = new RoundRectangle { CornerRadius = 16 },
                Stroke         = Color.FromArgb("#E2E8F0"),
                StrokeThickness= 1,
                BackgroundColor= Colors.White,
                WidthRequest   = 160,
                Padding        = new Thickness(0),
                Shadow         = new Shadow { Brush = Brush.Black, Offset = new Point(0, 3), Opacity = 0.07f, Radius = 8 }
            };

            var cardContent = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = 100 },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            // Hero image
            var imgBorder = new Border
            {
                StrokeShape     = new RoundRectangle { CornerRadius = new CornerRadius(16, 16, 0, 0) },
                Stroke          = Brush.Transparent,
                Content         = new Image { Source = poi.ImageSource, Aspect = Aspect.AspectFill }
            };
            Grid.SetRow(imgBorder, 0);
            cardContent.Children.Add(imgBorder);

            // Distance badge over image
            var distBadge = new Border
            {
                StrokeShape      = new RoundRectangle { CornerRadius = 8 },
                Stroke           = Brush.Transparent,
                BackgroundColor  = Color.FromArgb("#CC0F172A"),
                Padding          = new Thickness(8, 4),
                HorizontalOptions= LayoutOptions.End,
                VerticalOptions  = LayoutOptions.End,
                Margin           = new Thickness(0, 0, 6, 6),
                Content          = new Label { Text = poi.DistanceLabel, FontSize = 11, FontFamily = "OpenSans-Semibold", TextColor = Colors.White }
            };
            Grid.SetRow(distBadge, 0);
            cardContent.Children.Add(distBadge);

            // Info section
            var info = new VerticalStackLayout
            {
                Padding = new Thickness(10, 8, 10, 10),
                Spacing = 4
            };

            info.Children.Add(new Label
            {
                Text            = poi.Name,
                FontFamily      = "OpenSans-Semibold",
                FontSize        = 13,
                TextColor       = Color.FromArgb("#0F172A"),
                LineBreakMode   = LineBreakMode.TailTruncation
            });

            // Rating + open status row
            var metaRow = new HorizontalStackLayout { Spacing = 6 };
            metaRow.Children.Add(new Label { Text = poi.RatingLabel, FontSize = 11, FontFamily = "OpenSans-Regular", TextColor = Color.FromArgb("#F97316") });

            var openBadge = new Border
            {
                StrokeShape     = new RoundRectangle { CornerRadius = 6 },
                Stroke          = Brush.Transparent,
                BackgroundColor = poi.IsOpen ? Color.FromArgb("#D1FAE5") : Color.FromArgb("#FEE2E2"),
                Padding         = new Thickness(6, 2),
                Content         = new Label
                {
                    Text       = poi.IsOpen ? "Mở cửa" : "Đóng cửa",
                    FontSize   = 10,
                    FontFamily = "OpenSans-Semibold",
                    TextColor  = poi.IsOpen ? Color.FromArgb("#059669") : Color.FromArgb("#EF4444")
                }
            };
            metaRow.Children.Add(openBadge);
            info.Children.Add(metaRow);
            Grid.SetRow(info, 1);
            cardContent.Children.Add(info);

            card.Content = cardContent;

            // Tap → select POI and expand sheet
            var tap = new TapGestureRecognizer();
            tap.CommandParameter = poi;
            tap.Tapped += OnPoiCardTapped;
            card.GestureRecognizers.Add(tap);

            PoiCardStack.Children.Add(card);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  POI CARD TAP → EXPAND WITH DETAIL
    // ══════════════════════════════════════════════════════════════════════

    private async void OnPoiCardTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not PoiModel poi) return;
        _selectedPoi = poi;

        // Populate detail panel
        LblDetailPoiName.Text   = poi.Name;
        LblPoiName.Text         = poi.Name;
        DetailHeroImage.Source  = poi.ImageSource;
        MiniPlayerThumb.Source  = poi.ImageSource;
        DetailDescription.Text  = poi.Description;
        DetailRating.Text       = poi.RatingLabel;
        DetailDistance.Text     = poi.DistanceLabel;
        DetailStatus.Text       = poi.StatusLabel;
        DetailStatus.TextColor  = poi.StatusColor;
        DetailCategory.Text     = poi.Category;
        UpdateDetailAudioState(PoiService.AudioEnabled);

        // BR-008: If POI is closed, hide menu entirely
        if (poi.IsOpen)
        {
            MenuList.ItemsSource       = poi.Menu;
            MenuLabel.IsVisible        = true;
            MenuList.IsVisible         = true;
            //CtaButton.IsVisible        = true;
            ClosedNotice.IsVisible     = false;
            UpdateCtaSubtitle();
        }
        else
        {
            MenuLabel.IsVisible        = false;
            MenuList.IsVisible         = false;
            //CtaButton.IsVisible        = false;
            ClosedNotice.IsVisible     = true;
        }

        await ExpandSheet();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  TABS (Gần bạn / Đang phát)
    // ══════════════════════════════════════════════════════════════════════

    private void OnTabNearbyTapped(object sender, EventArgs e)
    {
        if (_showNearby) return;
        _showNearby = true;
        SetTabSelected(TabNearby, LblTabNearby, TabPlaying, LblTabPlaying);
        NearbyScroll.IsVisible    = true;
        MiniPlayerPanel.IsVisible = false;
    }

    private void OnTabPlayingTapped(object sender, EventArgs e)
    {
        if (!_showNearby) return;
        _showNearby = false;
        SetTabSelected(TabPlaying, LblTabPlaying, TabNearby, LblTabNearby);
        NearbyScroll.IsVisible    = false;
        MiniPlayerPanel.IsVisible = true;
    }

    private void SetTabSelected(Border selBorder, Label selLabel, Border unsBorder, Label unsLabel)
    {
        selBorder.BackgroundColor = Colors.Black;
        selBorder.Stroke          = Brush.Transparent;
        selLabel.TextColor        = Colors.White;
        selLabel.FontFamily       = "OpenSans-Semibold";

        unsBorder.BackgroundColor = Colors.White;
        unsBorder.Stroke          = new SolidColorBrush(Color.FromArgb("#E2E8F0"));
        unsLabel.TextColor        = Color.FromArgb("#94A3B8");
        unsLabel.FontFamily       = "OpenSans-Regular";
    }

    // ══════════════════════════════════════════════════════════════════════
    //  BOTTOM SHEET ANIMATIONS
    // ══════════════════════════════════════════════════════════════════════

    private void OnBottomSheetTap(object sender, EventArgs e) =>
        _ = _isExpanded ? CollapseSheet() : ExpandSheet();

    private async void OnBottomSheetPan(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _sheetStartY = BottomSheet.TranslationY;
                break;
            case GestureStatus.Running:
                double d = e.TotalY;
                if (!_isExpanded && d < 0) BottomSheet.TranslationY = Math.Max(_sheetStartY + d, -450);
                else if (_isExpanded && d > 0) BottomSheet.TranslationY = Math.Min(_sheetStartY + d, 0);
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                double moved = _sheetStartY - BottomSheet.TranslationY;
                if (!_isExpanded && moved > DRAG_THRESHOLD)       await ExpandSheet();
                else if (_isExpanded && moved < -DRAG_THRESHOLD)  await CollapseSheet();
                else await BottomSheet.TranslateToAsync(0, 0, 280, Easing.SpringOut);
                break;
        }
    }

    private async Task ExpandSheet()
    {
        _isExpanded = true;
        await BottomSheet.TranslateToAsync(0, 0, 200, Easing.SpringOut);
        ExpandedContent.IsVisible = true;
        await ExpandedContent.FadeToAsync(1, 220, Easing.CubicOut);
    }

    private async Task CollapseSheet()
    {
        _isExpanded = false;
        await ExpandedContent.FadeToAsync(0, 180, Easing.CubicIn);
        ExpandedContent.IsVisible = false;
        await BottomSheet.TranslateToAsync(0, 0, 200, Easing.SpringOut);
    }

    private async void OnBackTapped(object sender, EventArgs e) => await CollapseSheet();

    // ══════════════════════════════════════════════════════════════════════
    //  SEARCH
    // ══════════════════════════════════════════════════════════════════════

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        var q = e.NewTextValue?.ToLower() ?? "";
        PoiCardStack.Children.Clear();
        var pois = PoiService.GetSortedByDistance()
            .Where(p => string.IsNullOrEmpty(q) || p.Name.ToLower().Contains(q) || p.Category.ToLower().Contains(q));
        foreach (var poi in pois)
        {
            // Rebuild filtered cards (reuse BuildPoiCards pattern)
        }
        // Simple rebuild
        BuildPoiCards();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  CART
    // ══════════════════════════════════════════════════════════════════════

    private async void OnAddToCartTapped(object? sender, TappedEventArgs e)
    {
        if (_selectedPoi is null) return;
        if (e.Parameter is not FoodItem food) return;

        CartService.Instance.Add(_selectedPoi, food);

        // Quick feedback animation on the "+" button
        if (sender is Border btn)
        {
            await btn.ScaleToAsync(0.80, 70, Easing.CubicOut);
            await btn.ScaleToAsync(1.0,  120, Easing.SpringOut);
        }
        UpdateCtaSubtitle();
    }

    private void UpdateCartBadge()
    {
        int count = CartService.Instance.TotalCount;
        CartBadge.IsVisible    = count > 0;
        CartItemCount.Text     = count.ToString();
        UpdateCtaSubtitle();
    }

    private void UpdateCtaSubtitle()
    {
        /*int count = CartService.Instance.TotalCount;
        LblCtaSubtitle.Text = count == 0
            ? "Chưa có món nào"
            : $"{count} món • {CartService.Instance.TotalPriceLabel}";*/
    }

    private async void OnCartTapped(object sender, EventArgs e)
    {
        if (sender is Border btn)
        {
            await btn.ScaleToAsync(0.88, 80);
            await btn.ScaleToAsync(1.0, 120, Easing.SpringOut);
        }
        await Shell.Current.GoToAsync("//OrderPage");
    }

    private async void OnViewMenuTapped(object sender, EventArgs e)
    {
        if (sender is Border btn)
        {
            await btn.ScaleToAsync(0.97, 80, Easing.CubicOut);
            await btn.ScaleToAsync(1.0, 120, Easing.SpringOut);
        }
        await Shell.Current.GoToAsync("//OrderPage");
    }

    // ══════════════════════════════════════════════════════════════════════
    //  AUDIO
    // ══════════════════════════════════════════════════════════════════════

    private void OnPlayPauseTapped(object sender, EventArgs e)
    {
        _isPlaying = !_isPlaying;
        PlayPauseIcon.Text     = _isPlaying ? "\uf04c" : "\uf04b";
        LblAudioStatus.Text    = _isPlaying ? "Đang phát • 1:20 / 3:45" : "Đã tạm dừng";
        LblAudioStatus.TextColor = _isPlaying ? Color.FromArgb("#10B981") : Color.FromArgb("#EF4444");
        AudioDot.Color           = _isPlaying ? Color.FromArgb("#10B981") : Color.FromArgb("#EF4444");
    }

    private void OnAudioEnabledChanged(bool enabled)
    {
        // Update detail panel audio badge
        MainThread.BeginInvokeOnMainThread(() => UpdateDetailAudioState(enabled));

        if (!enabled)
        {
            _isPlaying = false;
            PlayPauseIcon.Text     = "\uf04b";
            LblAudioStatus.Text    = "Âm thanh đã tắt";
            LblAudioStatus.TextColor = Color.FromArgb("#EF4444");
            AudioDot.Color           = Color.FromArgb("#EF4444");
        }
    }

    private void UpdateDetailAudioState(bool enabled)
    {
        if (DetailAudioState is null) return;
        DetailAudioState.Text      = enabled ? "Audio đang phát" : "Audio đã tắt";
        DetailAudioState.TextColor = Colors.White;
    }

    private async void OnQueueTapped(object sender, EventArgs e)
    {
        // Navigate to Audio Player tab
        await Shell.Current.GoToAsync("//AudioPlayerPage");
    }

    // ══════════════════════════════════════════════════════════════════════
    //  MAP FABs 
    // ══════════════════════════════════════════════════════════════════════

    private async void OnMyLocationTapped(object sender, EventArgs e)
    {
        if (sender is Border btn) { await btn.ScaleToAsync(0.9, 80); await btn.ScaleToAsync(1.0, 120, Easing.SpringOut); }
        await CheckGpsAndConnectivity();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  NOTIFICATIONS
    // ══════════════════════════════════════════════════════════════════════

    private async void OnNotificationsTapped(object sender, EventArgs e)
    {
        if (sender is Border btn)
        {
            await btn.ScaleToAsync(0.9, 80);
            await btn.ScaleToAsync(1.0, 120, Easing.SpringOut);
        }
        await Shell.Current.GoToAsync("NotificationsPage");
    }

    private void UpdateNotificationBadge()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            int count = NotificationService.Instance.UnreadCount;
            NotificationBadge.IsVisible = count > 0;
            NotificationCount.Text = count > 9 ? "9+" : count.ToString();
        });
    }

    // ══════════════════════════════════════════════════════════════════════
    //  GPS SETTINGS (EC-H1)
    // ══════════════════════════════════════════════════════════════════════

    private void OnOpenGpsSettings(object sender, EventArgs e)
    {
        AppInfo.ShowSettingsUI();
    }

    // ══════════════════════════════════════════════════════════════════════
    //  LANGUAGE TOGGLE (on detail sheet)
    // ══════════════════════════════════════════════════════════════════════
    private void OnLangViTapped(object sender, EventArgs e) { }
    private void OnLangEnTapped(object sender, EventArgs e) { }
}
