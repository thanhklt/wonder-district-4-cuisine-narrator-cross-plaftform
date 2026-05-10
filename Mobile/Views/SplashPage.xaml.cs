using Mobile.ViewModels;

namespace Mobile.Views;

public partial class SplashPage : ContentPage
{
    private readonly SplashViewModel _vm;
    private bool _isChecking;

    public SplashPage(SplashViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Chống race condition khi OnAppearing bị gọi nhiều lần liên tiếp
        if (_isChecking) return;
        _isChecking = true;

        try
        {
            await _vm.CheckSessionCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SplashPage] OnAppearing error: {ex}");
            try { await Shell.Current.GoToAsync("//qrscan"); } catch { }
        }
        finally
        {
            _isChecking = false;
        }
    }
}
