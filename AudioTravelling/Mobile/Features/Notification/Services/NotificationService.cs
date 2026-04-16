using System.Collections.ObjectModel;
using AudioTravelling.Mobile.Features.Notification.Models;
using AudioTravelling.Mobile.Features.Poi.Models;

namespace AudioTravelling.Mobile.Features.Notification.Services;

public class NotificationService
{
    public static readonly NotificationService Instance = new();
    
    private NotificationService() { }

    private readonly ObservableCollection<NotificationModel> _notifications = new();
    
    public ObservableCollection<NotificationModel> Notifications => _notifications;

    public int UnreadCount => _notifications.Count(n => !n.IsRead);

    public event Action? UnreadCountChanged;

    public void AddOrderNotification(string poiName, string totalLabel)
    {
        var notif = new NotificationModel
        {
            Title = "Đặt món thành công",
            Message = $"Đơn hàng tại {poiName} đã được ghi nhận. Vui lòng đến cửa hàng để nhận món sau 15 phút.",
            Type = NotificationType.Order,
            OrderTotal = totalLabel
        };
        _notifications.Insert(0, notif);
        UnreadCountChanged?.Invoke();
    }

    public void AddArrivingAtPoiNotification(PoiModel poi)
    {
        if (_notifications.Any(n => n.Type == NotificationType.Location && n.RelatedPoiId == poi.PoiId))
            return;

        var notif = new NotificationModel
        {
            /*Title = "Bạn đã đến gần!",
            Message = $"Cách {poi.DistanceLabel} là {poi.Name}. Hãy mở Audio để nghe giới thiệu nhé.",
            Type = NotificationType.Location,
            RelatedPoiId = poi.Id*/
        };
        _notifications.Insert(0, notif);
        UnreadCountChanged?.Invoke();
    }

    public void MarkAllAsRead()
    {
        bool changed = false;
        foreach (var n in _notifications.Where(n => !n.IsRead))
        {
            n.IsRead = true;
            changed = true;
        }
        
        if (changed)
        {
            UnreadCountChanged?.Invoke();
        }
    }
}
