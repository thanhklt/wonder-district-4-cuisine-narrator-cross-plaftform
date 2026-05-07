using CommunityToolkit.Mvvm.Messaging;
using Mobile.Messages;
using Mobile.Models;
using Mobile.Services;
using Mobile.ViewModels;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using NetTopologySuite.Geometries;
using MapsuiBrush = Mapsui.Styles.Brush;
using MapsuiColor = Mapsui.Styles.Color;
using MapsuiPen = Mapsui.Styles.Pen;

namespace Mobile.Views;

public partial class MapPage : ContentPage, IRecipient<LocationUpdatedMessage>
{
    private readonly MapViewModel _vm;
    private MemoryLayer? _poiLayer;
    private MemoryLayer? _radiusLayer;
    private MemoryLayer? _userLayer;
    private IDispatcherTimer? _locationTimer;

    private const double VinhKhanhLat = 10.757;
    private const double VinhKhanhLon = 106.701;

    // Mau theo mobile-app
    private static readonly MapsuiColor OrangeFill    = new(249, 115, 22, 255);  // #f97316
    private static readonly MapsuiColor OrangeFaint   = new(249, 115, 22, 30);
    private static readonly MapsuiColor OrangeStroke  = new(249, 115, 22, 200);
    private static readonly MapsuiColor BlueFill      = new(59, 130, 246, 255);  // #3b82f6

    public MapPage(MapViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
        SetupMap();
    }

    // ── Map setup ────────────────────────────────────────────────────────────

    private void SetupMap()
    {
        // Tat log overlay (mac dinh la OnlyInDebugMode)
        Mapsui.Logging.Logger.LogDelegate = null;

        var map = new Mapsui.Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        // Tat log overlay (static property)
        Mapsui.Widgets.InfoWidgets.LoggingWidget.ShowLoggingInMap = Mapsui.Widgets.ActiveMode.No;

        _radiusLayer = new MemoryLayer { Name = "Radii", Style = null };
        _poiLayer    = new MemoryLayer { Name = "POIs",  Style = null };
        _userLayer   = new MemoryLayer { Name = "User",  Style = null };

        map.Layers.Add(_radiusLayer);
        map.Layers.Add(_poiLayer);
        map.Layers.Add(_userLayer);

        var (x, y) = SphericalMercator.FromLonLat(VinhKhanhLon, VinhKhanhLat);
        map.Navigator.CenterOnAndZoomTo(new MPoint(x, y), map.Navigator.Resolutions[17]);
        MapControl.Map = map;
    }

    // ── Lifecycle ────────────────────────────────────────────────────────────

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        MapControl.Info += OnMapInfo;
        WeakReferenceMessenger.Default.Register(this);

        try
        {
            await _vm.LoadPoisCommand.ExecuteAsync(null);

            System.Diagnostics.Debug.WriteLine($"[MapPage] Loaded {_vm.Pois.Count} POIs");

            await RefreshLayersAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MapPage.OnAppearing] {ex}");
        }

        StartLocationPolling();

        await Task.Delay(300);

        try { await StartForegroundServiceAsync(); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[MapPage.FgService] {ex}"); }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        MapControl.Info -= OnMapInfo;
        WeakReferenceMessenger.Default.Unregister<LocationUpdatedMessage>(this);
        _locationTimer?.Stop();
        _locationTimer = null;
    }

    // ── POI + Radius layers ──────────────────────────────────────────────────

    private async Task RefreshLayersAsync()
    {
        if (_poiLayer is null || _radiusLayer is null) return;

        var pois = _vm.Pois.ToList();

        var (poiFeatures, radiusFeatures) = await Task.Run(() => BuildFeatures(pois));

        _poiLayer.Features    = poiFeatures;
        _radiusLayer.Features = radiusFeatures;
        MapControl.Map?.RefreshData();

        System.Diagnostics.Debug.WriteLine($"[MapPage] Rendered {poiFeatures.Count} markers, {radiusFeatures.Count} radii");
    }

    private static (List<IFeature> pois, List<IFeature> radii) BuildFeatures(List<CachedPoi> pois)
    {
        var poiFeatures    = new List<IFeature>(pois.Count);
        var radiusFeatures = new List<IFeature>(pois.Count);

        foreach (var poi in pois)
        {
            var (x, y) = SphericalMercator.FromLonLat(poi.Longitude, poi.Latitude);

            // POI marker: cam (#f97316), SymbolStyle dam bao render duoc
            var marker = new PointFeature(new MPoint(x, y));
            marker["poi"] = poi;
            marker.Styles.Add(new SymbolStyle
            {
                Fill        = new MapsuiBrush(OrangeFill),
                Outline     = new MapsuiPen(MapsuiColor.White, 3f),
                SymbolScale = 1.4,
                SymbolType  = SymbolType.Ellipse
            });
            poiFeatures.Add(marker);

            // Vong tron geofence: cam nhat, vien dut khuc
            radiusFeatures.Add(CreateRadiusFeature(x, y, poi.Radius));
        }

        return (poiFeatures, radiusFeatures);
    }

    private static GeometryFeature CreateRadiusFeature(double cx, double cy, double radiusM)
    {
        const int seg = 48;
        var coords = new Coordinate[seg + 1];
        for (int i = 0; i <= seg; i++)
        {
            var a = 2 * Math.PI * i / seg;
            coords[i] = new Coordinate(cx + radiusM * Math.Cos(a), cy + radiusM * Math.Sin(a));
        }
        coords[seg] = coords[0];

        var feature = new GeometryFeature(new GeometryFactory().CreatePolygon(coords));
        feature.Styles.Add(new VectorStyle
        {
            Fill    = new MapsuiBrush(OrangeFaint),
            Outline = new MapsuiPen(OrangeStroke, 1.5f) { PenStyle = PenStyle.Dash }
        });
        return feature;
    }

    // ── User location ────────────────────────────────────────────────────────

    private void StartLocationPolling()
    {
        _locationTimer?.Stop();
        _locationTimer = Application.Current!.Dispatcher.CreateTimer();
        _locationTimer.Interval = TimeSpan.FromSeconds(3);
        _locationTimer.Tick += async (_, _) =>
        {
            try
            {
                var loc = await Geolocation.GetLastKnownLocationAsync();
                if (loc is not null)
                {
                    System.Diagnostics.Debug.WriteLine($"[MapPage] Location: {loc.Latitude:F5}, {loc.Longitude:F5}");
                    UpdateUserLayer(loc.Latitude, loc.Longitude);
                }
            }
            catch { }
        };
        _locationTimer.Start();
    }

    private void UpdateUserLayer(double lat, double lon)
    {
        if (_userLayer is null) return;
        var (x, y) = SphericalMercator.FromLonLat(lon, lat);

        var dot = new PointFeature(new MPoint(x, y));
        // Vong trang ngoai (border)
        dot.Styles.Add(new SymbolStyle
        {
            Fill        = new MapsuiBrush(MapsuiColor.White),
            Outline     = null,
            SymbolScale = 0.6,
            SymbolType  = SymbolType.Ellipse
        });
        // Dot xanh chinh
        dot.Styles.Add(new SymbolStyle
        {
            Fill        = new MapsuiBrush(BlueFill),
            Outline     = null,
            SymbolScale = 0.42,
            SymbolType  = SymbolType.Ellipse
        });

        _userLayer.Features = [dot];
        MapControl.Map?.RefreshData();
    }

    public void Receive(LocationUpdatedMessage message) =>
        UpdateUserLayer(message.Latitude, message.Longitude);

    // ── Map tap ──────────────────────────────────────────────────────────────

    private void OnMapInfo(object? sender, MapInfoEventArgs e)
    {
        if (_vm.IsPoiPopupVisible)
        {
            MainThread.BeginInvokeOnMainThread(() => _vm.ClosePoiPopupCommand.Execute(null));
            return;
        }

        try
        {
            var world = e.WorldPosition;
            if (world is null) return;
            var wx = world.X; var wy = world.Y;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var (lon, lat) = SphericalMercator.ToLonLat(wx, wy);
                    const double tol = 0.0005;
                    var nearest = _vm.Pois
                        .Where(p => Math.Abs(p.Latitude - lat) < tol
                                 && Math.Abs(p.Longitude - lon) < tol)
                        .OrderBy(p => Math.Pow(p.Latitude - lat, 2) + Math.Pow(p.Longitude - lon, 2))
                        .FirstOrDefault();

                    if (nearest is not null)
                        _ = _vm.ShowPoiPopupCommand.ExecuteAsync(nearest);
                }
                catch { }
            });
        }
        catch { }
    }

    private async Task StartForegroundServiceAsync()
    {
        var started = await GeofenceServiceStarter.StartAsync(requestPermission: true);
        if (!started)
            _vm.StatusMessage = "Cần quyền vị trí để tự động phát audio theo GPS.";
    }
}
