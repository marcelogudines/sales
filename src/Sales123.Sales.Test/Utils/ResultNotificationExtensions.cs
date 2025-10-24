using FluentAssertions;
using Sales123.Sales.Domain.Abstractions;

namespace Sales123.Sales.Test;

public static class ResultNotificationExtensions
{
    public static void ShouldHaveNotification<T>(this Result<T> r, string code) =>
        r.Notifications.Items.Should().Contain(n => n.Code == code);
}
