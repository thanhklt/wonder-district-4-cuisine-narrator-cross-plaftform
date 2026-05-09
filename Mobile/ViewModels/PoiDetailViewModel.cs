using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Models;
using Mobile.Services;

namespace Mobile.ViewModels;

[QueryProperty(nameof(Poi), "Poi")]
public partial class PoiDetailViewModel : BaseViewModel
{
    private readonly AudioService _audio;
    private readonly DatabaseService _db;
    private readonly ApiService _api;

    [ObservableProperty]
    private CachedPoi? _poi;

    [ObservableProperty]
    private string _localizedName = string.Empty;

    [ObservableProperty]
    private string _localizedDescription = string.Empty;

    [ObservableProperty]
    private bool _isAudioPlaying;

    [ObservableProperty]
    private ObservableCollection<string> _imageUrls = [];

    [ObservableProperty]
    private bool _hasMultipleImages;

    public PoiDetailViewModel(AudioService audio, DatabaseService db, ApiService api)
    {
        _audio = audio;
        _db = db;
        _api = api;
    }

    partial void OnPoiChanged(CachedPoi? value)
    {
        if (value is null) return;
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await LoadLocalizationAsync();
            await LoadImagesAsync();
        });
    }

    private async Task LoadLocalizationAsync()
    {
        if (Poi is null) return;
        var lang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var loc = await _db.GetLocalizationAsync(Poi.PoiID, lang);

        LocalizedName = Poi.PoiName;                                      // luôn tiếng Việt
        LocalizedDescription = loc?.Description ?? Poi.DescriptionVi;    // ngôn ngữ điện thoại → vi
    }

    private async Task LoadImagesAsync()
    {
        if (Poi is null) return;

        // Luôn hiển thị ảnh bìa ngay (offline-safe)
        if (!string.IsNullOrEmpty(Poi.CoverImageUrl))
            ImageUrls = new ObservableCollection<string>([Poi.CoverImageUrl]);

        // Nếu online → tải full gallery
        var imgs = await _api.GetPoiImagesAsync(Poi.PoiID);
        if (imgs is { Count: > 0 })
        {
            var urls = imgs.OrderBy(i => i.DisplayOrder)
                           .Select(i => i.ImageUrl)
                           .Where(u => !string.IsNullOrEmpty(u))
                           .ToList();
            if (urls.Count > 0)
                ImageUrls = new ObservableCollection<string>(urls);
        }

        HasMultipleImages = ImageUrls.Count > 1;
    }

    [RelayCommand]
    public async Task PlayAudioAsync()
    {
        if (Poi is null) return;
        var lang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        await _audio.PlayAsync(Poi, lang, "manual");
        IsAudioPlaying = true;
    }

    [RelayCommand]
    public void StopAudio()
    {
        _audio.Stop();
        IsAudioPlaying = false;
    }

    [RelayCommand]
    public static async Task GoBackAsync() =>
        await Shell.Current.GoToAsync("..");
}
