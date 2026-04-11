using System.Windows.Input;
using AudioTravelling.Mobile.Services.Api.Interfaces;
using AudioTravelling.Mobile.ViewModels;

namespace AudioTravelling.Mobile.Features.Auth.ViewModels;

public class ProfileViewModel : BaseViewModel
{
    private readonly IUserApiService _userApiService;

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    private string _fullName = string.Empty;
    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    private string _role = string.Empty;
    public string Role
    {
        get => _role;
        set => SetProperty(ref _role, value);
    }

    public ICommand LoadMeCommand { get; }

    public ProfileViewModel(IUserApiService userApiService)
    {
        _userApiService = userApiService;
        LoadMeCommand = new Command(async () => await LoadMeAsync());
    }

    private async Task LoadMeAsync()
    {
        try
        {
            IsBusy = true;

            var me = await _userApiService.GetMeAsync();

            if (me == null)
            {
                await Shell.Current.DisplayAlertAsync("Lỗi", "Không lấy được thông tin user.", "OK");
                return;
            }

            Email = me.Email;
            FullName = me.FullName;
            Role = me.Role;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Lỗi", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}