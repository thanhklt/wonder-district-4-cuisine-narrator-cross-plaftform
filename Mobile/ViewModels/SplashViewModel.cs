using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile.Services;

namespace Mobile.ViewModels;

public partial class SplashViewModel : BaseViewModel
{
    private readonly SessionService _session;
    private readonly ApiService _api;

    public SplashViewModel(SessionService session, ApiService api)
    {
        _session = session;
        _api = api;
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
                    // Sync POI se chay ngam trong MapPage.OnAppearing
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
}
