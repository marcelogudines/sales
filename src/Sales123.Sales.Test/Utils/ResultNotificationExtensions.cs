using Sales123.Sales.Domain.Abstractions;
using Shouldly;

namespace Sales123.Sales.Test;

public static class ResultNotificationExtensions
{
    public static void ShouldHaveNotification<T>(this Result<T> r, string code) =>
        r.Notifications.Items.ShouldContain(n => n.Code == code);

    public static void ShouldHaveSingleNotification<T>(this Result<T> r, string code) =>
        r.Notifications.Items.Count(n => n.Code == code).ShouldBe(1);
}
