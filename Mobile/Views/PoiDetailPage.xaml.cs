using Mobile.ViewModels;

namespace Mobile.Views;

public partial class PoiDetailPage : ContentPage
{
    public PoiDetailPage(PoiDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
