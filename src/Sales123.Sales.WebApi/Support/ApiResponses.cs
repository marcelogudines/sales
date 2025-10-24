
using Sales123.Sales.Domain.Notifications;
using Sales123.Sales.Domain.Shared;

namespace Sales123.Sales.WebApi.Support;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? TraceId { get; init; }
    public IEnumerable<Notification>? Notifications { get; init; }

    public static ApiResponse<T> Ok(T data, string traceId)
        => new() { Success = true, Data = data, TraceId = traceId };
    public static ApiResponse<T> Error(NotificationsBag notification, string traceId)
        => new() { Success = false, Data = default, TraceId = traceId, Notifications = notification.Items };
}
