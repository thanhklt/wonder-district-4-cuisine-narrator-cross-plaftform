using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Services;

namespace Mobile.ViewModels;

public partial class SplashViewModel : BaseViewModel
{
    private readonly SessionService _session;
    private readonly ApiService _api;
    private readonly DatabaseService _db;

    public SplashViewModel(SessionService session, ApiService api, DatabaseService db)
    {
        _session = session;
        _api = api;
        _db = db;
    }

    [RelayCommand]
    public async Task CheckSessionAsync()
    {
        IsBusy = true;
        StatusMessage = "Đang kiểm tra phiên truy cập...";

        try
        {
            var session = await _session.GetValidSessionAsync();

            if (session is not null)
            {
                var verified = await _api.VerifySessionAsync(session.SessionID, session.DeviceID);
                if (verified?.Valid == true)
                {
                    StatusMessage = "Đang tải dữ liệu...";
                    await SyncBootstrapAsync();
                    await Shell.Current.GoToAsync("//map");
                    return;
                }
                await _session.InvalidateAsync();
            }

            await Shell.Current.GoToAsync("//qrscan");
        }
        catch
        {
            var session = await _session.GetValidSessionAsync();
            if (session is not null)
            {
                await Shell.Current.GoToAsync("//map");
                return;
            }
            await Shell.Current.GoToAsync("//qrscan");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SyncBootstrapAsync()
    {
        try
        {
            var data = await _api.BootstrapAsync();
            if (data is null) return;

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
        }
        catch { }
    }
}
