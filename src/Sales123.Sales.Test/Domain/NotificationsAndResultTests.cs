using FluentAssertions;
using Sales123.Sales.Domain.Abstractions;
using Sales123.Sales.Domain.Notifications;
using Sales123.Sales.Domain.Shared;
using Xunit;

namespace Sales123.Sales.Test.Domain;

public class NotificationsAndResultTests
{
    [Fact]
    public void NotificationsBag_add_merge_and_prefix()
    {
        var bag1 = new NotificationsBag();
        bag1.Add("A", "a", "p1");
        bag1.Add(new Notification("B", "b", "p2", NotificationSeverity.Warning));

        var bag2 = new NotificationsBag();
        bag2.Add("C", "c", "p3");
        bag2.Add("D", "d", "p4");

        bag1.Merge(bag2, pathPrefix: "root");

        bag1.Items.Should().HaveCount(4);
        bag1.Items.Should().Contain(n => n.Code == "C" && n.Path == "root.p3");
    }

    [Fact]
    public void Result_ok_and_fail()
    {
        var ok = Result.Ok(123);
        ok.IsValid.Should().BeTrue();
        ok.Value.Should().Be(123);

        var fail = Result<int>.Fail(new Notification("E", "e", "p"));
        fail.IsValid.Should().BeFalse();
        fail.Notifications.Items.Should().ContainSingle(n => n.Code == "E");
    }
}
