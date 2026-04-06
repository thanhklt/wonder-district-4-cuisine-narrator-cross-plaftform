using Mobile.Models;
using Mobile.Services;

namespace Mobile.Views;

public partial class OrderPage : ContentPage
{
    public OrderPage()
    {
        InitializeComponent();
        CartService.Instance.CartChanged += RefreshUI;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshUI();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Keep subscription alive (page stays in Shell)
    }

    private void RefreshUI()
    {
        var items = CartService.Instance.Items;
        bool hasItems = items.Count > 0;

        EmptyState.IsVisible  = !hasItems;
        CartList.IsVisible    = hasItems;
        CheckoutBar.IsVisible = hasItems;

        CartList.ItemsSource  = null;
        CartList.ItemsSource  = CartService.Instance.GroupedItems;

        LblTotal.Text     = CartService.Instance.TotalPriceLabel;
    }

    private void OnDecreaseQty(object sender, EventArgs e)
    {
        if (sender is not BindableObject b || b.BindingContext is not OrderItem item) return;
        CartService.Instance.Remove(item);
    }

    private void OnIncreaseQty(object sender, EventArgs e)
    {
        if (sender is not BindableObject b || b.BindingContext is not OrderItem item) return;
        CartService.Instance.Add(item.Poi, item.Food);
    }

    private async void OnClearCartTapped(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlertAsync("Xóa giỏ hàng?", "Tất cả món sẽ bị xóa.", "Xóa", "Hủy");
        if (confirm) CartService.Instance.Clear();
    }

    private async void OnCheckoutTapped(object sender, EventArgs e)
    {
        if (sender is Border btn)
        {
            await btn.ScaleToAsync(0.96, 80, Easing.CubicOut);
            await btn.ScaleToAsync(1.0, 120, Easing.SpringOut);
        }

        // EC-O3: Check connectivity before placing order
        var connectivity = Connectivity.NetworkAccess;
        if (connectivity != NetworkAccess.Internet)
        {
            await DisplayAlertAsync("Không thể đặt hàng",
                "Kiểm tra kết nối mạng và thử lại.", "OK");
            return;
        }

        string total = CartService.Instance.TotalPriceLabel;
        
        // Tạo notification cho từng POI trong giỏ hàng
        foreach (var group in CartService.Instance.GroupedItems)
        {
            NotificationService.Instance.AddOrderNotification(group.PoiName, group.Sum(i => i.LineTotal).ToString("#,##0") + " VNĐ");
        }

        CartService.Instance.Clear();
        await DisplayAlertAsync("Đặt hàng thành công!",
            $"Đơn hàng đã được ghi nhận. Vui lòng kiểm tra thông báo để biết chi tiết.", "OK");

        // BR-019: Navigate to Home after order
        await Shell.Current.GoToAsync("//MainPage");
    }

    private async void OnExploreTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}
