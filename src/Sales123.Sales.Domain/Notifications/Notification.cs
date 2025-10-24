
namespace Sales123.Sales.Domain.Notifications;

public enum NotificationSeverity { Error = 0, Warning = 1, Info = 2 }

public sealed record Notification(
    string Code,
    string Message,
    string? Path = null,
    NotificationSeverity Severity = NotificationSeverity.Error);
