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

        // Build POI cards
        BuildPoiCards();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateCartBadge();
        UpdateCtaSubtitle();
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
        MenuList.ItemsSource    = poi.Menu;
        UpdateDetailAudioState(PoiService.AudioEnabled);
        UpdateCtaSubtitle();

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
        selBorder.BackgroundColor = Color.FromArgb("#10B981");
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
                else await BottomSheet.TranslateTo(0, 0, 280, Easing.SpringOut);
                break;
        }
    }

    private async Task ExpandSheet()
    {
        _isExpanded = true;
        await BottomSheet.TranslateTo(0, 0, 200, Easing.SpringOut);
        ExpandedContent.IsVisible = true;
        await ExpandedContent.FadeTo(1, 220, Easing.CubicOut);
    }

    private async Task CollapseSheet()
    {
        _isExpanded = false;
        await ExpandedContent.FadeTo(0, 180, Easing.CubicIn);
        ExpandedContent.IsVisible = false;
        await BottomSheet.TranslateTo(0, 0, 200, Easing.SpringOut);
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
            await btn.ScaleTo(0.80, 70, Easing.CubicOut);
            await btn.ScaleTo(1.0,  120, Easing.SpringOut);
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
        int count = CartService.Instance.TotalCount;
        LblCtaSubtitle.Text = count == 0
            ? "Chưa có món nào"
            : $"{count} món • {CartService.Instance.TotalPriceLabel}";
    }

    private async void OnCartTapped(object sender, EventArgs e)
    {
        if (sender is Border btn)
        {
            await btn.ScaleTo(0.88, 80);
            await btn.ScaleTo(1.0, 120, Easing.SpringOut);
        }
        await Shell.Current.GoToAsync("//OrderPage");
    }

    private async void OnViewMenuTapped(object sender, EventArgs e)
    {
        if (sender is Border btn)
        {
            await btn.ScaleTo(0.97, 80, Easing.CubicOut);
            await btn.ScaleTo(1.0, 120, Easing.SpringOut);
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
        LblAudioStatus.TextColor = _isPlaying ? Color.FromArgb("#059669") : Color.FromArgb("#94A3B8");
        AudioDot.Color           = _isPlaying ? Color.FromArgb("#10B981") : Color.FromArgb("#CBD5E1");
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
        DetailAudioState.Text      = enabled ? "&#xF028;  Audio đang phát" : "&#xF6A9;  Audio đã tắt";
        DetailAudioState.TextColor = enabled ? Color.FromArgb("#059669") : Color.FromArgb("#94A3B8");
    }

    private void OnQueueTapped(object sender, EventArgs e) =>
        DisplayAlert("Danh sách phát", "Takoyaki Kukuru\nOctopus Ball Street (tiếp theo)\nDotonbori Night Walk", "Đóng");

    // ══════════════════════════════════════════════════════════════════════
    //  MAP FABs
    // ══════════════════════════════════════════════════════════════════════

    private async void OnMyLocationTapped(object sender, EventArgs e)
    {
        if (sender is Border btn) { await btn.ScaleTo(0.9, 80); await btn.ScaleTo(1.0, 120, Easing.SpringOut); }
    }

    // ══════════════════════════════════════════════════════════════════════
    //  LANGUAGE TOGGLE (on detail sheet)
    // ══════════════════════════════════════════════════════════════════════
    private void OnLangViTapped(object sender, EventArgs e) { }
    private void OnLangEnTapped(object sender, EventArgs e) { }
}
