using FluentAssertions;
using Sales123.Sales.Domain.Aggregates;
using Sales123.Sales.Domain.Entities;
using Sales123.Sales.Domain.Events;
using Sales123.Sales.Domain.ValueObjects;
using Sales123.Sales.Test.Utils;
using Xunit;

namespace Sales123.Sales.Test.Domain;

public class SaleAggregateTests
{
    private static (CustomerRef, BranchRef, List<SaleItemInput>) Inputs(int items = 1)
    {
        var cust = TestHelper.Customer();
        var branch = TestHelper.Branch();
        var list = new List<SaleItemInput>();
        for (int i = 0; i < items; i++) list.Add(TestHelper.ItemInput(qty: i + 1, unit: 10m));
        return (cust, branch, list);
    }

    [Fact]
    public void Create_valid_sale_raises_event_and_sums_total()
    {
        var (c, b, items) = Inputs(items: 2);
        var res = Sale.Create("N-1", DateTimeOffset.UtcNow, c, b, items);
        res.IsValid.Should().BeTrue();

        var sale = res.Value!;
        sale.Number.Should().Be("N-1");
        sale.Items.Count.Should().Be(2);
        sale.SaleTotal.Value.Should().Be(30m); 

        var evts = sale.DequeueEvents().ToList();
        evts.Should().ContainSingle().Which.Should().BeOfType<SaleCreated>();
        sale.DequeueEvents().Should().BeEmpty();
    }

    [Fact]
    public void Create_header_validations_and_items_min_1()
    {
        var r1 = Sale.Create(null, DateTimeOffset.UtcNow, TestHelper.Customer(), TestHelper.Branch(), Enumerable.Empty<SaleItemInput>());
        r1.IsValid.Should().BeFalse();
        r1.Notifications.Items.Should().Contain(n => n.Code == "sale.number_required");

        var r2 = Sale.Create("N", DateTimeOffset.UtcNow, customer: null!, TestHelper.Branch(), Enumerable.Empty<SaleItemInput>());
        r2.IsValid.Should().BeFalse();
        r2.Notifications.Items.Should().Contain(n => n.Code == "sale.customer_required");

        var r3 = Sale.Create("N", DateTimeOffset.UtcNow, TestHelper.Customer(), branch: null!, Enumerable.Empty<SaleItemInput>());
        r3.IsValid.Should().BeFalse();
        r3.Notifications.Items.Should().Contain(n => n.Code == "sale.branch_required");

        var r4 = Sale.Create("N", DateTimeOffset.UtcNow, TestHelper.Customer(), TestHelper.Branch(), Enumerable.Empty<SaleItemInput>());
        r4.IsValid.Should().BeFalse();
        r4.Notifications.Items.Should().Contain(n => n.Code == "sale.items_min_1");
    }

    [Fact]
    public void AddItem_replace_cancelitem_and_cancel_sale()
    {
        var (c, b, items) = Inputs(items: 1);
        var sale = Sale.Create("N", DateTimeOffset.UtcNow, c, b, items).Value!;

        var add = sale.AddItem(TestHelper.ItemInput(qty: 5, unit: 10m));
        add.IsValid.Should().BeTrue();
        sale.Items.Count.Should().Be(2);

        var it = sale.Items.Last();
        var repl = sale.ReplaceItemQuantity(it.Id, 10);
        repl.IsValid.Should().BeTrue();
        it.Quantity.Should().Be(10);
        it.DiscountPercent.Should().Be(20);

        var first = sale.Items.First();
        var cancelItem = sale.CancelItem(first.Id);
        cancelItem.IsValid.Should().BeTrue();
        first.Canceled.Should().BeTrue();

        sale.SaleTotal.Value.Should().Be(it.ItemTotal.Value);

        var cancel = sale.Cancel("motivo");
        cancel.IsValid.Should().BeTrue();

        var evts = sale.DequeueEvents().ToList();
        evts.Should().Contain(e => e is SaleUpdated);
        evts.Should().Contain(e => e is SaleItemCanceled);
        evts.Should().Contain(e => e is SaleCanceled);

        sale.Cancel(null).IsValid.Should().BeTrue();  
        sale.DequeueEvents().Should().BeEmpty();
    }

    [Fact]
    public void Guard_on_canceled_sale_blocks_mutations()
    {
        var sale = Sale.Create("N", DateTimeOffset.UtcNow, TestHelper.Customer(), TestHelper.Branch(), new[] { TestHelper.ItemInput() }).Value!;
        sale.Cancel(null);

        sale.AddItem(TestHelper.ItemInput()).IsValid.Should().BeFalse();
        sale.ReplaceItemQuantity(sale.Items.First().Id, 2).IsValid.Should().BeFalse();
        sale.CancelItem(sale.Items.First().Id).IsValid.Should().BeFalse();

        sale.Notifications.Should().NotBeNull();
    }

    [Fact]
    public void Replace_and_cancel_item_not_found()
    {
        var sale = Sale.Create("N", DateTimeOffset.UtcNow, TestHelper.Customer(), TestHelper.Branch(), new[] { TestHelper.ItemInput() }).Value!;
        sale.ReplaceItemQuantity("X", 1).IsValid.Should().BeFalse();

        var r = sale.CancelItem("X");
        r.IsValid.Should().BeFalse();
        r.Notifications.Items.Should().Contain(n => n.Code == "sale.item_not_found");
    }
}
