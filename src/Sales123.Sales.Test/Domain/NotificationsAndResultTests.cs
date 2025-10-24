using Shouldly;
using Sales123.Sales.Domain.Abstractions;
using Sales123.Sales.Domain.Notifications;
using Sales123.Sales.Domain.Shared;
using Xunit;

namespace Sales123.Sales.Test.Domain;

public class NotificationsAndResultTests
{
    [Fact]
    public void NotificationsBag_add_merge_with_prefix()
    {
        var baseBag = new NotificationsBag();
        baseBag.Add("A", "a", "p1");
        baseBag.Add(new Notification("B", "b", "p2", NotificationSeverity.Warning));

        var toMerge = new NotificationsBag();
        toMerge.Add("C", "c", "p3");

        baseBag.Merge(toMerge, "root");

        baseBag.Items.Count.ShouldBe(3);
        baseBag.Items.ShouldContain(n => n.Code == "A" && n.Path == "p1");
        baseBag.Items.ShouldContain(n => n.Code == "B" && n.Path == "p2");
        baseBag.Items.ShouldContain(n => n.Code == "C" && n.Path == "root.p3");
    }

    [Fact]
    public void Result_ok_and_fail()
    {
        var ok = Result.Ok(123);
        ok.IsValid.ShouldBeTrue();
        ok.Value.ShouldBe(123);

        var fail = Result<int>.Fail(new Notification("E", "e", "p"));
        fail.IsValid.ShouldBeFalse();
        fail.Notifications.Items.ShouldContain(n => n.Code == "E");
    }
}
