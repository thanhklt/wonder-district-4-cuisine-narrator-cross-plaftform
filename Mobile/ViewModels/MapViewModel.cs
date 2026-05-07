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

    [ObservableProperty] private ObservableCollection<CachedPoi> _pois = [];
    [ObservableProperty] private string _lastTriggeredPoiName = string.Empty;
    [ObservableProperty] private bool _isAudioPlaying;

    // POI popup state
    [ObservableProperty] private bool _isPoiPopupVisible;
    [ObservableProperty] private string _popupPoiName = string.Empty;
    [ObservableProperty] private string _popupDescription = string.Empty;
    [ObservableProperty] private string _popupCoverImageUrl = string.Empty;
    [ObservableProperty] private bool _hasPopupImage;
    [ObservableProperty] private CachedPoi? _selectedPoi;

    public MapViewModel(DatabaseService db, AudioService audio, SessionService session)
    {
        _db = db;
        _audio = audio;
        _session = session;
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
        PopupCoverImageUrl = poi.CoverImageUrl ?? string.Empty;
        HasPopupImage = !string.IsNullOrEmpty(poi.CoverImageUrl);

        var lang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var loc = await _db.GetLocalizationAsync(poi.PoiID, lang)
                  ?? await _db.GetLocalizationAsync(poi.PoiID, "en");

        PopupPoiName = loc?.Name ?? poi.PoiName;
        PopupDescription = loc?.Description ?? poi.DescriptionVi;
        IsPoiPopupVisible = true;
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

    public void Receive(GeofenceTriggeredMessage message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LastTriggeredPoiName = message.PoiName;
            IsAudioPlaying = true;
        });
    }
}
