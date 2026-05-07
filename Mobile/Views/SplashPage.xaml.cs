using Mobile.ViewModels;

namespace Mobile.Views;

public partial class SplashPage : ContentPage
{
    private readonly SplashViewModel _vm;

    public SplashPage(SplashViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.CheckSessionCommand.ExecuteAsync(null);
    }
}
