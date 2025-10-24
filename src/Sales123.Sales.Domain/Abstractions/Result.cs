
using DomainNotification = Sales123.Sales.Domain.Notifications.Notification;
using Sales123.Sales.Domain.Shared;

namespace Sales123.Sales.Domain.Abstractions;

public sealed class Result<T>
{
    private Result(T? value, NotificationsBag notification)
    {
        Value = value;
        Notifications = notification;
    }

    public T? Value { get; }
    public NotificationsBag Notifications { get; }
    public bool IsValid => !Notifications.HasErrors;

    public static Result<T> Ok(T value) => new(value, new NotificationsBag());

    public static Result<T> Fail(params DomainNotification[] notifications)
    {
        var notification = new NotificationsBag();
        foreach (var n in notifications) notification.Add(n);
        return new(default, notification);
    }

    public static Result<T> From(T? value, NotificationsBag notification) => new(value, notification);
}

public static class Result
{
    public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);
    public static Result<T> Fail<T>(params DomainNotification[] n) => Result<T>.Fail(n);
}
