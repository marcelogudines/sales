using FluentAssertions;
using Sales123.Sales.Domain.ValueObjects;
using Xunit;

namespace Sales123.Sales.Test.Domain;

public class MoneyTests
{
    [Fact]
    public void From_null_returns_error()
    {
        var res = Money.From(null);
        res.IsValid.Should().BeFalse();
        res.Notifications.Items.Should().ContainSingle(n => n.Code == "money.required" && n.Path == "value");
    }

    [Fact]
    public void From_negative_returns_error()
    {
        var res = Money.From(-1m);
        res.IsValid.Should().BeFalse();
        res.Notifications.Items.Should().ContainSingle(n => n.Code == "money.non_negative");
    }

    [Fact]
    public void Rounds_away_from_zero_to_two_decimals()
    {
        Money.FromRaw(10.005m).Value.Should().Be(10.01m);
        Money.FromRaw(10.004m).Value.Should().Be(10.00m);
    }

    [Fact]
    public void Operators_work()
    {
        var a = Money.FromRaw(10m);
        var b = Money.FromRaw(2.5m);

        (a + b).Value.Should().Be(12.50m);
        (a * 3).Value.Should().Be(30m);
        (b * 4m).Value.Should().Be(10m);
    }
}
