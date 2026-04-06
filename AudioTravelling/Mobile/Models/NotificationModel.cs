namespace Mobile.Models;

public enum NotificationType
{
    Order,
    Location
}

public class NotificationModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Now;
    public bool IsRead { get; set; } = false;
    public NotificationType Type { get; set; } = NotificationType.Location;
    
    // Khóa ngoại để lấy thông tin phụ nếu cần (vd ID của Poi)
    public int? RelatedPoiId { get; set; }
    
    // Khóa phụ cho Order
    public string? OrderTotal { get; set; }

    // Helpers cho View
    public string FormattedDate => Date.ToString("dd/MM/yyyy HH:mm");
    
    public string Icon => Type == NotificationType.Order ? "\uf290" : "\uf3c5"; // FA shopping-bag / location-dot
    
    public Color IconBgColor => Type == NotificationType.Order 
        ? Color.FromArgb("#DBEAFE") 
        : Color.FromArgb("#FEF3C7");
    
    public Color IconColor => Type == NotificationType.Order 
        ? Color.FromArgb("#2563EB") 
        : Color.FromArgb("#D97706");
}
