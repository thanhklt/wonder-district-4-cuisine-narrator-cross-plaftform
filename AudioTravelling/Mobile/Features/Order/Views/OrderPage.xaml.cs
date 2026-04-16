using AudioTravelling.Mobile.Features.Notification.Services;
using AudioTravelling.Mobile.Features.Order.Models;
using AudioTravelling.Mobile.Features.Order.Services;
using AudioTravelling.Mobile.Features.Poi.Models;

namespace AudioTravelling.Mobile.Features.Order.Views;

public partial class OrderPage : ContentPage
{
    private readonly CartService _cart = CartService.Instance;
    private readonly NotificationService _notifSvc = NotificationService.Instance;

    public OrderPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshCart();
    }

    private void RefreshCart()
    {
        bool hasItems = _cart.TotalCount > 0;
        EmptyState.IsVisible   = !hasItems;
        CartList.IsVisible     = hasItems;
        CheckoutBar.IsVisible  = hasItems;

        if (hasItems)
        {
            CartList.ItemsSource = _cart.GroupedItems;
            LblTotal.Text = _cart.TotalPriceLabel;
        }
    }

    private void OnIncreaseQty(object sender, EventArgs e)
    {
        if (sender is not BindableObject bo) return;
        if (bo.BindingContext is not OrderItem item) return;
        _cart.Add(item.Poi, item.Food);
        RefreshCart();
    }

    private void OnDecreaseQty(object sender, EventArgs e)
    {
        if (sender is not BindableObject bo) return;
        if (bo.BindingContext is not OrderItem item) return;
        _cart.Remove(item);
        RefreshCart();
    }

    private async void OnClearCartTapped(object sender, EventArgs e)
    {
        if (_cart.TotalCount == 0) return;
        bool confirm = await DisplayAlert("Xác nhận", "Bạn muốn xóa toàn bộ giỏ hàng?", "Xóa", "Hủy");
        if (!confirm) return;
        _cart.Clear();
        RefreshCart();
    }

    private async void OnCheckoutTapped(object sender, EventArgs e)
    {
        if (_cart.TotalCount == 0) return;

        // Get the first POI name for the notification
        string poiName = _cart.Items.FirstOrDefault()?.Poi.NameVi ?? "Dotonbori";
        string totalLabel = _cart.TotalPriceLabel;

        // Add order notification
        _notifSvc.AddOrderNotification(poiName, totalLabel);

        await DisplayAlert("Đặt hàng thành công!",
            $"Tổng: {_cart.TotalPriceLabel}\nVui lòng đến cửa hàng để nhận món sau 15 phút.",
            "OK");

        _cart.Clear();
        RefreshCart();
    }
}
