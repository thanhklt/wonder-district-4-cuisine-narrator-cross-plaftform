using Mobile.Models;
using Mobile.Services;

namespace Mobile.Views;

public partial class NotificationsPage : ContentPage
{
    public NotificationsPage()
    {
        InitializeComponent();
        
        NotificationList.ItemsSource = NotificationService.Instance.Notifications;
        UpdateEmptyState();

        NotificationService.Instance.Notifications.CollectionChanged += (s, e) => UpdateEmptyState();
    }

    private void UpdateEmptyState()
    {
        bool hasItems = NotificationService.Instance.Notifications.Any();
        EmptyState.IsVisible = !hasItems;
        NotificationList.IsVisible = hasItems;
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PopAsync();
    }

    private void OnMarkAllReadTapped(object sender, EventArgs e)
    {
        NotificationService.Instance.MarkAllAsRead();
        // Since we don't have full INotifyPropertyChanged on isRead in the simplest setup,
        // we force refresh the collection view by re-assigning
        NotificationList.ItemsSource = null;
        NotificationList.ItemsSource = NotificationService.Instance.Notifications;
    }

    private async void OnNotificationSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not NotificationModel notif)
            return;

        // Mark as read
        notif.IsRead = true;
        NotificationService.Instance.MarkAllAsRead(); // just to force UnreadCount update
        
        NotificationList.ItemsSource = null;
        NotificationList.ItemsSource = NotificationService.Instance.Notifications;

        // Show details
        if (notif.Type == NotificationType.Order)
        {
            await DisplayAlertAsync(notif.Title, $"Chi tiết đơn hàng:\n{notif.Message}\nTổng tiền: {notif.OrderTotal}", "Đóng");
        }
        else if (notif.Type == NotificationType.Location)
        {
            await DisplayAlertAsync(notif.Title, notif.Message, "Đóng");
            // Could navigate to POI if needed, but for MVP DisplayAlert is fine.
        }

        NotificationList.SelectedItem = null;
    }
}
