using AudioTravelling.Mobile.Features.Poi.ViewModels;

namespace AudioTravelling.Mobile.Features.Poi.Views;

public partial class PoiListPage : ContentPage
{
    public PoiListPage(PoiListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            if (BindingContext is PoiListViewModel vm && vm.Pois.Count == 0)
            {
                await vm.LoadPoisAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PoiListPage.OnAppearing] {ex}");
            await DisplayAlert("Lỗi", "Không thể tải dữ liệu địa điểm.", "OK");
        }
    }
}