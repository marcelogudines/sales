using Shouldly;
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
        res.IsValid.ShouldBeTrue();

        var item = res.Value!;
        item.DiscountPercent.ShouldBe(10);
        item.ItemTotal.Value.ShouldBe(450m);
    }

    [Fact]
    public void Create_validates_requireds_and_ranges()
    {
        var res1 = SaleItem.Create(product: null!, 1, TestHelper.Money(10m));
        res1.IsValid.ShouldBeFalse();
        res1.Notifications.Items.ShouldContain(n => n.Code == "item.product_required");

        var res2 = SaleItem.Create(TestHelper.Product(), 0, TestHelper.Money(10m));
        res2.IsValid.ShouldBeFalse();
        res2.Notifications.Items.ShouldContain(n => n.Code == "item.quantity_range");

        var res3 = SaleItem.Create(TestHelper.Product(), 1, unitPrice: null!);
        res3.IsValid.ShouldBeFalse();
        res3.Notifications.Items.ShouldContain(n => n.Code == "item.unit_price_required");
    }

    [Fact]
    public void Replace_quantity_recalculates_and_validates()
    {
        var item = SaleItem.Create(TestHelper.Product(), 1, TestHelper.Money(100m)).Value!;
        var bad = item.ReplaceQuantity(21);
        bad.IsValid.ShouldBeFalse();
        bad.Notifications.Items.ShouldContain(n => n.Code == "item.quantity_range");

        var ok = item.ReplaceQuantity(10);
        ok.IsValid.ShouldBeTrue();
        item.DiscountPercent.ShouldBe(20);
        item.ItemTotal.Value.ShouldBe(800m);
    }

    [Fact]
    public void Cancel_sets_flag()
    {
        var item = SaleItem.Create(TestHelper.Product(), 1, TestHelper.Money(10m)).Value!;
        item.Cancel();
        item.Canceled.ShouldBeTrue();
    }
}
