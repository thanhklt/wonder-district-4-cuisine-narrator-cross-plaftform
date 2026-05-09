using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mobile.Messages;
using Mobile.Models;
using Mobile.Services;
using System.Collections.ObjectModel;

namespace Mobile.ViewModels;

public partial class MapViewModel : BaseViewModel, IRecipient<GeofenceTriggeredMessage>
{
    private readonly DatabaseService _db;
    private readonly AudioService _audio;
    private readonly SessionService _session;
    private readonly ApiService _api;

    [ObservableProperty] private ObservableCollection<CachedPoi> _pois = [];
    [ObservableProperty] private string _lastTriggeredPoiName = string.Empty;
    [ObservableProperty] private bool _isAudioPlaying;

    // POI popup state
    [ObservableProperty] private bool _isPoiPopupVisible;
    [ObservableProperty] private string _popupPoiName = string.Empty;
    [ObservableProperty] private string _popupDescription = string.Empty;
    [ObservableProperty] private string _popupCoverImageUrl = string.Empty;
    [ObservableProperty] private bool _hasPopupImage;
    [ObservableProperty] private bool _popupHasMultipleImages;
    [ObservableProperty] private ObservableCollection<string> _popupImageUrls = [];
    [ObservableProperty] private CachedPoi? _selectedPoi;

    public MapViewModel(DatabaseService db, AudioService audio, SessionService session, ApiService api)
    {
        _db = db;
        _audio = audio;
        _session = session;
        _api = api;
        WeakReferenceMessenger.Default.Register(this);
    }

    [RelayCommand]
    public async Task LoadPoisAsync()
    {
        var list = await _db.GetActivePoisAsync();
        Pois = new ObservableCollection<CachedPoi>(list);
    }

    [RelayCommand]
    public async Task ShowPoiPopupAsync(CachedPoi poi)
    {
        SelectedPoi = poi;
        PopupPoiName = poi.PoiName;

        // Hiện ảnh bìa ngay lập tức (offline-safe)
        var coverUrl = poi.CoverImageUrl ?? string.Empty;
        PopupCoverImageUrl = coverUrl;
        HasPopupImage = !string.IsNullOrEmpty(coverUrl);
        PopupImageUrls = string.IsNullOrEmpty(coverUrl)
            ? []
            : new ObservableCollection<string>([coverUrl]);
        PopupHasMultipleImages = false;

        var lang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var loc = await _db.GetLocalizationAsync(poi.PoiID, lang);

        PopupPoiName = poi.PoiName;                                   // luôn tiếng Việt
        PopupDescription = loc?.Description ?? poi.DescriptionVi;     // ngôn ngữ điện thoại → vi
        IsPoiPopupVisible = true;

        // Load full gallery in background (nếu online)
        _ = LoadPopupImagesAsync(poi.PoiID);
    }

    private async Task LoadPopupImagesAsync(int poiId)
    {
        var imgs = await _api.GetPoiImagesAsync(poiId);
        if (imgs is not { Count: > 0 }) return;

        var urls = imgs.OrderBy(i => i.DisplayOrder)
                       .Select(i => i.ImageUrl)
                       .Where(u => !string.IsNullOrEmpty(u))
                       .ToList();
        if (urls.Count == 0) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            PopupImageUrls = new ObservableCollection<string>(urls);
            HasPopupImage = true;
            PopupHasMultipleImages = urls.Count > 1;
        });
    }

    [RelayCommand]
    public void ClosePoiPopup()
    {
        IsPoiPopupVisible = false;
        SelectedPoi = null;
    }

    [RelayCommand]
    public async Task PlayPopupPoiAsync()
    {
        if (SelectedPoi is null) return;
        var lang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        await _audio.PlayAsync(SelectedPoi, lang, "manual");
        IsAudioPlaying = true;
        LastTriggeredPoiName = PopupPoiName;
    }

    [RelayCommand]
    public void StopAudio()
    {
        _audio.Stop();
        IsAudioPlaying = false;
    }

    // Giu lai de tuong thich (khong dung nua nhung tranh loi tham chieu cu)
    [RelayCommand]
    public async Task PlayPoiManuallyAsync(CachedPoi poi)
    {
        var langCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        await _audio.PlayAsync(poi, langCode, "manual");
        IsAudioPlaying = true;
        LastTriggeredPoiName = poi.PoiName;
    }

    // Goi bootstrap API va upsert vao SQLite, tra ve true neu co du lieu moi
    public async Task<bool> SyncFromApiAsync()
    {
        try
        {
            var data = await _api.BootstrapAsync();
            if (data is null) return false;

            var pois = data.Pois.Select(p => new Models.CachedPoi
            {
                PoiID = p.PoiID, PoiName = p.PoiName, DescriptionVi = p.DescriptionVi,
                Latitude = p.Latitude, Longitude = p.Longitude,
                Radius = p.Radius, Priority = p.Priority,
                IsActive = p.IsActive, CoverImageUrl = p.CoverImageUrl,
                UpdatedDate = p.UpdatedDate, CachedAt = DateTime.UtcNow
            });
            await _db.UpsertPoisAsync(pois);

            var locs = data.Localizations.Select(l => new Models.CachedPoiLocalization
            {
                LocalizationID = l.LocalizationID, PoiID = l.PoiID,
                LanguageCode = l.LanguageCode, Name = l.Name,
                Description = l.Description, AudioUrl = l.AudioUrl,
                UpdatedDate = l.UpdatedDate, CachedAt = DateTime.UtcNow
            });
            await _db.UpsertLocalizationsAsync(locs);

            // Cap nhat Pois collection de map tu dong refresh
            var list = await _db.GetActivePoisAsync();
            Pois = new System.Collections.ObjectModel.ObservableCollection<Models.CachedPoi>(list);
            return true;
        }
        catch { return false; }
    }

    public void Receive(GeofenceTriggeredMessage message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LastTriggeredPoiName = message.PoiName;
            IsAudioPlaying = true;
        });
    }
}
