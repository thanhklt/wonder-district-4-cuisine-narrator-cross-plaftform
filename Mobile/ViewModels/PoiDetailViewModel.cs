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

    [ObservableProperty]
    private CachedPoi? _poi;

    [ObservableProperty]
    private string _localizedName = string.Empty;

    [ObservableProperty]
    private string _localizedDescription = string.Empty;

    [ObservableProperty]
    private bool _isAudioPlaying;

    public PoiDetailViewModel(AudioService audio, DatabaseService db)
    {
        _audio = audio;
        _db = db;
    }

    partial void OnPoiChanged(CachedPoi? value)
    {
        if (value is null) return;
        MainThread.BeginInvokeOnMainThread(async () => await LoadLocalizationAsync());
    }

    private async Task LoadLocalizationAsync()
    {
        if (Poi is null) return;
        var lang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var loc = await _db.GetLocalizationAsync(Poi.PoiID, lang)
                  ?? await _db.GetLocalizationAsync(Poi.PoiID, "en");

        LocalizedName = loc?.Name ?? Poi.PoiName;
        LocalizedDescription = loc?.Description ?? Poi.DescriptionVi;
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
