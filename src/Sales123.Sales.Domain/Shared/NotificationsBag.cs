using DomainNotification = Sales123.Sales.Domain.Notifications.Notification;
using DomainSeverity = Sales123.Sales.Domain.Notifications.NotificationSeverity;

namespace Sales123.Sales.Domain.Shared;

public sealed class NotificationsBag
{
    private readonly List<DomainNotification> _list = new();
    public IReadOnlyList<DomainNotification> Items => _list;
    public bool HasErrors => _list.Any(n => n.Severity == DomainSeverity.Error);

    public void Add(string code, string message, string? path = null, DomainSeverity severity = DomainSeverity.Error)
        => _list.Add(new DomainNotification(code, message, path, severity));

    public void Add(DomainNotification notification) => _list.Add(notification);

    public void Merge(NotificationsBag other, string? pathPrefix = null)
    {
        if (other is null) return;
        foreach (var n in other.Items)
        {
            var path = string.IsNullOrWhiteSpace(pathPrefix) ? n.Path
                : string.IsNullOrWhiteSpace(n.Path) ? pathPrefix
                : $"{pathPrefix}.{n.Path}";
            _list.Add(n with { Path = path });
        }
    }
}

public abstract class Notifiable
{
    protected readonly NotificationsBag _notifications = new();
    public IReadOnlyList<DomainNotification> Notifications => _notifications.Items;
    public bool IsValid => !_notifications.HasErrors;
}
