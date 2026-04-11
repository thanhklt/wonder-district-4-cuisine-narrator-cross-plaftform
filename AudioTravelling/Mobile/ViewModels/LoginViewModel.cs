using System.Windows.Input;
using AudioTravelling.Mobile.Services.Api.Interfaces;
using AudioTravelling.Mobile.Services.Api.Requests;
using AudioTravelling.Mobile.Services.Auth;
using AudioTravelling.Mobile.ViewModels;

namespace AudioTravelling.Mobile.Features.Auth.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly IAuthApiService _authApiService;
    private readonly ITokenService _tokenService;

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private string _emailError = string.Empty;
    public string EmailError
    {
        get => _emailError;
        set
        {
            if (SetProperty(ref _emailError, value))
                OnPropertyChanged(nameof(HasEmailError));
        }
    }

    public bool HasEmailError => !string.IsNullOrWhiteSpace(EmailError);

    private string _passwordError = string.Empty;
    public string PasswordError
    {
        get => _passwordError;
        set
        {
            if (SetProperty(ref _passwordError, value))
                OnPropertyChanged(nameof(HasPasswordError));
        }
    }

    public bool HasPasswordError => !string.IsNullOrWhiteSpace(PasswordError);

    private string _generalError = string.Empty;
    public string GeneralError
    {
        get => _generalError;
        set
        {
            if (SetProperty(ref _generalError, value))
                OnPropertyChanged(nameof(HasGeneralError));
        }
    }

    public bool HasGeneralError => !string.IsNullOrWhiteSpace(GeneralError);

    public ICommand LoginCommand { get; }

    public LoginViewModel(
        IAuthApiService authApiService,
        ITokenService tokenService)
    {
        _authApiService = authApiService ?? throw new ArgumentNullException(nameof(authApiService));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));

        LoginCommand = new Command(async () => await LoginAsync());
    }

    private bool Validate()
    {
        EmailError = string.Empty;
        PasswordError = string.Empty;
        GeneralError = string.Empty;

        var isValid = true;

        if (string.IsNullOrWhiteSpace(Email))
        {
            EmailError = "Email không được để trống.";
            isValid = false;
        }
        else if (!Email.Contains("@"))
        {
            EmailError = "Email không đúng định dạng.";
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            PasswordError = "Mật khẩu không được để trống.";
            isValid = false;
        }
        else if (Password.Length < 6)
        {
            PasswordError = "Mật khẩu phải có ít nhất 6 ký tự.";
            isValid = false;
        }

        return isValid;
    }

    private async Task LoginAsync()
    {
        if (IsBusy)
            return;

        if (!Validate())
            return;

        try
        {
            IsBusy = true;
            GeneralError = string.Empty;

            var request = new LoginRequest
            {
                Email = Email.Trim(),
                Password = Password
            };

            var result = await _authApiService.LoginAsync(request);

            if (result == null)
            {
                GeneralError = "API trả về rỗng.";
                return;
            }

            if (string.IsNullOrWhiteSpace(result.AccessToken))
            {
                GeneralError = "Response không có token.";
                return;
            }

            await _tokenService.SaveTokenAsync(result.AccessToken);

            await ShowSuccessMessageAsync();
            await NavigateToMainAsync();
        }
        catch (Exception ex)
        {
            GeneralError = ex.InnerException?.Message ?? ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static async Task ShowSuccessMessageAsync()
    {
        if (Shell.Current != null)
        {
            await Shell.Current.DisplayAlert("Thành công", "Đăng nhập thành công.", "OK");
            return;
        }

        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert("Thành công", "Đăng nhập thành công.", "OK");
        }
    }

    private async Task NavigateToMainAsync()
    {
        try
        {
            // Replace the main page with AppShell to enable shell navigation
            if (Application.Current != null)
            {
                Application.Current.MainPage = new AppShell();
                return;
            }

            GeneralError = "Không thể khởi tạo trang chính.";
        }
        catch (Exception ex)
        {
            GeneralError = $"Lỗi điều hướng: {ex.Message}";
        }
    }
}