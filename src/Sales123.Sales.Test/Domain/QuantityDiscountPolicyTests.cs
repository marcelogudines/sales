using FluentAssertions;
using Sales123.Sales.Domain.Policies;
using Xunit;

namespace Sales123.Sales.Test.Domain;

public class QuantityDiscountPolicyTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(20, true)]
    [InlineData(21, false)]
    public void IsAllowed_range(int qty, bool allowed)
        => QuantityDiscountPolicy.IsAllowed(qty).Should().Be(allowed);

    [Theory]
    [InlineData(1, 0)]
    [InlineData(4, 0)]
    [InlineData(5, 10)]
    [InlineData(9, 10)]
    [InlineData(10, 20)]
    [InlineData(15, 20)]
    public void Discount_for_quantity(int qty, int expected)
        => QuantityDiscountPolicy.DiscountPercentFor(qty).Should().Be(expected);
}
