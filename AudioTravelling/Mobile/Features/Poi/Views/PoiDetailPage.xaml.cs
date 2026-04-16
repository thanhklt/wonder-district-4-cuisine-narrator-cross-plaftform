using AudioTravelling.Mobile.Features.Poi.ViewModels;

namespace AudioTravelling.Mobile.Features.Poi.Views;

public partial class PoiDetailPage : ContentPage
{
    public PoiDetailPage(PoiDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnBackTapped(object sender, TappedEventArgs e)
    {
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync("..");
        else
            await Navigation.PopAsync();
    }
}