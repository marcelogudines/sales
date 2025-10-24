using Shouldly;
using Sales123.Sales.Domain.ValueObjects;
using Xunit;

namespace Sales123.Sales.Test.Domain;

public class MoneyTests
{
    [Fact]
    public void From_null_returns_error()
    {
        var res = Money.From(null);
        res.IsValid.ShouldBeFalse();
        res.Notifications.Items.Count(n => n.Code == "money.required" && n.Path == "value").ShouldBe(1);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-10)]
    public void From_negative_returns_error(decimal value)
    {
        var res = Money.From(value);
        res.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Large_values_are_valid_if_non_negative()
    {
        var res = Money.From(1_000_000_000_000m); 
        res.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Rounding_and_ops()
    {
        Money.FromRaw(10.004m).Value.ShouldBe(Money.FromRaw(10.00m).Value);

        var a = Money.FromRaw(10m);
        var b = Money.FromRaw(2.5m);

        (a + b).Value.ShouldBe(12.50m);
        (a * 3).Value.ShouldBe(30m);
        (b * 4m).Value.ShouldBe(10m);
    }
}
