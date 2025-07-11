using DKR.Core.Entities;

namespace DKR.Core.Interfaces;

public interface INotificationService
{
    event Action<Notification> OnNotification;
    Task NotifyAsync(string title, string message, NotificationType type = NotificationType.Info);
    Task NotifyEmergencyAsync(string title, string message, EmergencyType emergencyType);
    Task SendAsync(NotificationRequest request);
}

public class Notification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Danger,
    Emergency
}

public class NotificationRequest
{
    public Shared.Enums.NotificationChannel Channel { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}