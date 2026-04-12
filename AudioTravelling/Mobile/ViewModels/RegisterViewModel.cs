using System.Windows.Input;
using AudioTravelling.Mobile.Services.Api.Interfaces;
using AudioTravelling.Mobile.Services.Api.Requests;
using AudioTravelling.Mobile.ViewModels;

namespace AudioTravelling.Mobile.Features.Auth.ViewModels;

public class RegisterViewModel : BaseViewModel
{
    private readonly IAuthApiService _authApiService;

    public RegisterViewModel(IAuthApiService authApiService)
    {
        _authApiService = authApiService;
        RegisterCommand = new Command(async () => await RegisterAsync(), () => !IsBusy);
    }

    private string _fullName = string.Empty;
    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

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

    private string _confirmPassword = string.Empty;
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    private string _message = string.Empty;
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public ICommand RegisterCommand { get; }

    private async Task RegisterAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Message = string.Empty;

            if (string.IsNullOrWhiteSpace(FullName))
            {
                Message = "Vui lòng nhập họ tên.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                Message = "Vui lòng nhập email.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                Message = "Vui lòng nhập mật khẩu.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                Message = "Mật khẩu xác nhận không khớp.";
                return;
            }

            var request = new RegisterRequest
            {
                FullName = FullName,
                Email = Email,
                Password = Password
            };

            await _authApiService.RegisterAsync(request);

            Message = "Đăng ký thành công.";

            // Có thể điều hướng sang trang Login ở đây nếu muốn
            // await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (Exception ex)
        {
            Message = "Đăng ký thất bại. Vui lòng thử lại.";
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}