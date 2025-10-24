using Shouldly;
using Sales123.Sales.Domain.Aggregates;
using Sales123.Sales.Domain.Entities;
using Sales123.Sales.Domain.Events;
using Sales123.Sales.Domain.ValueObjects;
using Sales123.Sales.Domain.Enums;
using Sales123.Sales.Test.Utils;
using Xunit;

namespace Sales123.Sales.Test.Domain;

public class SaleAggregateTests
{
    private static (CustomerRef, BranchRef, List<SaleItemInput>) Inputs(int items = 1)
    {
        var cust = TestHelper.Customer();
        var branch = TestHelper.Branch();
        var inputs = Enumerable.Range(0, items)
            .Select(_ => TestHelper.ItemInput(qty: 1, unit: 10m))
            .ToList();
        return (cust, branch, inputs);
    }

    [Fact]
    public void Create_ok_raises_event()
    {
        var (cust, branch, inputs) = Inputs(1);
        var r = Sale.Create("N", DateTimeOffset.UtcNow, cust, branch, inputs);
        r.IsValid.ShouldBeTrue();
        var sale = r.Value!;
        var evts = sale.DequeueEvents();
        evts.ShouldNotBeNull();
        evts!.Any(e => e is SaleCreated).ShouldBeTrue();
    }

    [Fact]
    public void Create_header_validations_and_items_min_1()
    {
        var r1 = Sale.Create(null, DateTimeOffset.UtcNow, TestHelper.Customer(), TestHelper.Branch(), Enumerable.Empty<SaleItemInput>());
        r1.IsValid.ShouldBeFalse();
        r1.Notifications.Items.ShouldContain(n => n.Code == "sale.number_required");

        var r2 = Sale.Create("N", DateTimeOffset.UtcNow, customer: null!, TestHelper.Branch(), Enumerable.Empty<SaleItemInput>());
        r2.IsValid.ShouldBeFalse();
        r2.Notifications.Items.ShouldContain(n => n.Code == "sale.customer_required");

        var r3 = Sale.Create("N", DateTimeOffset.UtcNow, TestHelper.Customer(), branch: null!, Enumerable.Empty<SaleItemInput>());
        r3.IsValid.ShouldBeFalse();
        r3.Notifications.Items.ShouldContain(n => n.Code == "sale.branch_required");

        var r4 = Sale.Create("N", DateTimeOffset.UtcNow, TestHelper.Customer(), TestHelper.Branch(), Enumerable.Empty<SaleItemInput>());
        r4.IsValid.ShouldBeFalse();
        r4.Notifications.Items.ShouldContain(n => n.Code == "sale.items_min_1");
    }

    [Fact]
    public void Add_replace_cancel_item_and_cancel_sale()
    {
        var (cust, branch, inputs) = Inputs(1);
        var sale = Sale.Create("N", DateTimeOffset.UtcNow, cust, branch, inputs).Value!;

        var addRes = sale.AddItem(TestHelper.ItemInput(qty: 5, unit: 100m));
        addRes.IsValid.ShouldBeTrue();
        sale.Items.Count.ShouldBe(2);

        var firstId = sale.Items.First().Id;
        var rep = sale.ReplaceItemQuantity(firstId, 10);
        rep.IsValid.ShouldBeTrue();

        var can = sale.CancelItem(firstId);
        can.IsValid.ShouldBeTrue();

        var cancelSale = sale.Cancel("user_request");
        cancelSale.IsValid.ShouldBeTrue();
        sale.Status.ShouldBe(SaleStatus.Canceled);

        var evts = sale.DequeueEvents();
        evts.ShouldNotBeNull();
    }

    [Fact]
    public void Replace_and_cancel_item_not_found()
    {
        var sale = Sale.Create("N", DateTimeOffset.UtcNow, TestHelper.Customer(), TestHelper.Branch(), new[] { TestHelper.ItemInput() }).Value!;
        sale.ReplaceItemQuantity("X", 1).IsValid.ShouldBeFalse();

        var r = sale.CancelItem("X");
        r.IsValid.ShouldBeFalse();
        r.Notifications.Items.ShouldContain(n => n.Code == "sale.item_not_found");
    }
}
