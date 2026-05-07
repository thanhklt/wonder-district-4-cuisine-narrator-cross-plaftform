using Mobile.Views;

namespace Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("poiDetail", typeof(PoiDetailPage));
    }
}
