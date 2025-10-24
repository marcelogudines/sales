using FluentAssertions;
using Sales123.Sales.Domain.Entities;
using Sales123.Sales.Test.Utils;
using Xunit;

namespace Sales123.Sales.Test.Domain;

public class SaleItemTests
{
    [Fact]
    public void Create_valid_item_calculates_discount_and_total()
    {
        var res = SaleItem.Create(TestHelper.Product(), 5, TestHelper.Money(100m));
        res.IsValid.Should().BeTrue();

        var item = res.Value!;
        item.DiscountPercent.Should().Be(10);
        item.ItemTotal.Value.Should().Be(450m);
        item.Canceled.Should().BeFalse();
        item.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Create_valid_item_no_discount_when_qty_lt5()
    {
        var item = SaleItem.Create(TestHelper.Product(), 1, TestHelper.Money(10m)).Value!;
        item.DiscountPercent.Should().Be(0);
        item.ItemTotal.Value.Should().Be(10m);
    }

    [Fact]
    public void Create_valid_item_discount_20_when_qty_ge10()
    {
        var item = SaleItem.Create(TestHelper.Product(), 10, TestHelper.Money(10m)).Value!;
        item.DiscountPercent.Should().Be(20);
        item.ItemTotal.Value.Should().Be(80m);
    }

    [Fact]
    public void Create_validation_errors()
    {
        var res1 = SaleItem.Create(product: null!, 1, TestHelper.Money(10m));
        res1.IsValid.Should().BeFalse();
        res1.Notifications.Items.Should().Contain(n => n.Code == "item.product_required");

        var res2 = SaleItem.Create(TestHelper.Product(), 0, TestHelper.Money(10m));
        res2.IsValid.Should().BeFalse();
        res2.Notifications.Items.Should().Contain(n => n.Code == "item.quantity_range");

        var res3 = SaleItem.Create(TestHelper.Product(), 1, unitPrice: null!);
        res3.IsValid.Should().BeFalse();
        res3.Notifications.Items.Should().Contain(n => n.Code == "item.unit_price_required");
    }

    [Fact]
    public void Replace_quantity_recalculates_and_validates()
    {
        var item = SaleItem.Create(TestHelper.Product(), 1, TestHelper.Money(100m)).Value!;
        var bad = item.ReplaceQuantity(0);
        bad.IsValid.Should().BeFalse();

        var ok = item.ReplaceQuantity(10);
        ok.IsValid.Should().BeTrue();
        item.DiscountPercent.Should().Be(20);
        item.ItemTotal.Value.Should().Be(800m);
    }

    [Fact]
    public void Cancel_sets_flag()
    {
        var item = SaleItem.Create(TestHelper.Product(), 1, TestHelper.Money(10m)).Value!;
        item.Cancel();
        item.Canceled.Should().BeTrue();
    }
}
