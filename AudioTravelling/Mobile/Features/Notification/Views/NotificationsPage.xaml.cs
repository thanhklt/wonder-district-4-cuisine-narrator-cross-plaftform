using AudioTravelling.Mobile.Features.Notification.Services;
using AudioTravelling.Mobile.Features.Notification.Models;

namespace AudioTravelling.Mobile.Features.Notification.Views;

public partial class NotificationsPage : ContentPage
{
    private readonly NotificationService _notifSvc = NotificationService.Instance;

    public NotificationsPage()
    {
        InitializeComponent();

        // Converter for read/unread background color
        Resources.Add("BooleanToObjectConverter", new BoolToObjectConverter
        {
            TrueObject  = Color.FromArgb("#F8FAFC"),
            FalseObject = Colors.White
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshList();
    }

    private void RefreshList()
    {
        var items = _notifSvc.Notifications;
        EmptyState.IsVisible       = items.Count == 0;
        NotificationList.IsVisible = items.Count > 0;
        NotificationList.ItemsSource = items;
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private void OnMarkAllReadTapped(object sender, EventArgs e)
    {
        _notifSvc.MarkAllAsRead();
        RefreshList();
    }

    private void OnNotificationSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is NotificationModel notif)
        {
            notif.IsRead = true;
            ((CollectionView)sender).SelectedItem = null;
            RefreshList();
        }
    }
}

// Simple bool to Color converter
public class BoolToObjectConverter : IValueConverter
{
    public object? TrueObject  { get; set; }
    public object? FalseObject { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => value is true ? TrueObject : FalseObject;

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => throw new NotImplementedException();
}
