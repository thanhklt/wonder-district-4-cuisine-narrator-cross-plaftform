using System.Windows.Input;
using AudioTravelling.Mobile.Services.Api.Exceptions;
using AudioTravelling.Mobile.Services.Api.Interfaces;
using AudioTravelling.Mobile.Services.Api.Requests;
using AudioTravelling.Mobile.Services.Auth;
using AudioTravelling.Mobile.ViewModels;
using AudioTravelling.Mobile.Core.Sync;

namespace AudioTravelling.Mobile.Features.Auth.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly IAuthApiService _authApiService;
    private readonly ITokenService _tokenService;
    private readonly ISyncService _syncService;

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
    ITokenService tokenService,
    ISyncService syncService)   // 👈 thêm vào đây
    {
        _authApiService = authApiService ?? throw new ArgumentNullException(nameof(authApiService));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));

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
            EmailError = "Vui lòng nhập email.";
            isValid = false;
        }
        else if (!Email.Contains("@"))
        {
            EmailError = "Email chưa đúng định dạng.";
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            PasswordError = "Vui lòng nhập mật khẩu.";
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
                GeneralError = "Đăng nhập chưa thành công. Vui lòng thử lại.";
                return;
            }

            if (string.IsNullOrWhiteSpace(result.AccessToken))
            {
                GeneralError = "Đăng nhập chưa hoàn tất. Vui lòng thử lại.";
                return;
            }

            await _tokenService.SaveTokenAsync(result.AccessToken);

            // 🔥 CALL SYNC HERE
            var syncResult = await _syncService.SyncBootstrapAsync("en");

            if (!syncResult.IsSuccess)
            {
                GeneralError = syncResult.ErrorMessage;
                return;
            }

            // Debug để kiểm tra
            System.Diagnostics.Debug.WriteLine($"SYNC OK: {syncResult.PoiCount} POIs");

            // Sau đó mới navigate
            await NavigateToMainAsync();
        }
        catch (ApiException ex)
        {
            GeneralError = ex.UserMessage;
        }
        catch
        {
            GeneralError = "Có lỗi xảy ra. Vui lòng thử lại sau.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Task NavigateToMainAsync()
    {
        try
        {
            var window = Application.Current?.Windows.FirstOrDefault();
            if (window is not null)
            {
                window.Page = new AppShell();
                return Task.CompletedTask;
            }

            GeneralError = "Đăng nhập thành công, nhưng chưa tìm thấy cửa sổ ứng dụng.";
        }
        catch
        {
            GeneralError = "Đăng nhập thành công, nhưng chưa thể chuyển màn hình. Vui lòng mở lại ứng dụng.";
        }

        return Task.CompletedTask;
    }
}