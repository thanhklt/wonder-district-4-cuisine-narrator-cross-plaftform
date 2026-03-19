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
        CartList.ItemsSource  = CartService.Instance.Items;

        LblItemCount.Text = $"{CartService.Instance.TotalCount} món";
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
        bool confirm = await DisplayAlert("Xóa giỏ hàng?", "Tất cả món sẽ bị xóa.", "Xóa", "Hủy");
        if (confirm) CartService.Instance.Clear();
    }

    private async void OnCheckoutTapped(object sender, EventArgs e)
    {
        if (sender is Border btn)
        {
            await btn.ScaleTo(0.96, 80, Easing.CubicOut);
            await btn.ScaleTo(1.0, 120, Easing.SpringOut);
        }

        string total = CartService.Instance.TotalPriceLabel;
        CartService.Instance.Clear();
        await DisplayAlert("🎉 Đặt hàng thành công!", $"Đơn hàng {total} đã được ghi nhận.\nThời gian chuẩn bị: ~15 phút.", "OK");
    }
}
