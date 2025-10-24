using NSubstitute;
using Sales123.Sales.Application.DTOs;
using Sales123.Sales.Domain.Aggregates;
using Shouldly;
using Xunit;

namespace Sales123.Sales.Test;

public class SaleServiceTests
{
    [Fact]
    public void Create_persists_and_publishes_when_new()
    {
        var sut = new SaleServiceSut();
        sut.Repo.AddIfNotExists(Arg.Any<Sale>())
                .Returns(ci => (true, (Sale)ci[0]));
        var service = sut.Build();

        var res = service.Create(Mothers.ACreateSaleCommand());
        res.IsValid.ShouldBeTrue();

        sut.Repo.Received(1).AddIfNotExists(Arg.Any<Sale>());
        sut.ShouldPublishEventsOnce();
    }

    [Fact]
    public void Create_does_not_publish_when_exists()
    {
        var sut = new SaleServiceSut();
        sut.Repo.AddIfNotExists(Arg.Any<Sale>())
                .Returns(ci => (false, (Sale)ci[0]));
        var service = sut.Build();

        var res = service.Create(Mothers.ACreateSaleCommand());

        res.IsValid.ShouldBeFalse();
        res.ShouldHaveNotification("sale.already_exists");

        sut.Repo.Received(1).AddIfNotExists(Arg.Any<Sale>());
        sut.ShouldNotPublish();
    }

    [Fact]
    public void Delete_delegates_to_repository()
    {
        var sut = new SaleServiceSut();
        sut.Repo.Delete("X").Returns(true);
        var service = sut.Build();

        service.Delete("X").ShouldBeTrue();
        sut.Repo.Received(1).Delete("X");
    }

    [Fact]
    public void AddItem_fails_when_sale_not_found()
    {
        var sut = new SaleServiceSut();
        sut.Query.GetById(Arg.Any<string>()).Returns(x => (Sale)null!);
        var service = sut.Build();

        var res = service.AddItem(Mothers.AnAddItem("S-404"));

        res.IsValid.ShouldBeFalse();
        res.ShouldHaveNotification("sale.not_found");
        sut.Repo.DidNotReceive().Update(Arg.Any<Sale>());
        sut.ShouldNotPublish();
    }

    [Fact]
    public void AddItem_updates_and_publishes()
    {
        var sut = new SaleServiceSut();
        var sale = Mothers.AExistingSale(out var _);
        sut.Query.GetById(sale.Id).Returns(sale);
        var service = sut.Build();

        var res = service.AddItem(Mothers.AnAddItem(sale.Id));
        res.IsValid.ShouldBeTrue();

        sut.Repo.Received(1).Update(sale);
        sut.ShouldPublishEventsOnce();
    }

    [Fact]
    public void ReplaceItemQuantity_and_CancelItem_update_and_publish()
    {
        var sut = new SaleServiceSut();
        var sale = Mothers.AExistingSale(out var firstItemId);
        sut.Query.GetById(sale.Id).Returns(sale);
        var service = sut.Build();

        var r1 = service.ReplaceItemQuantity(new ReplaceItemQuantityCommand(sale.Id, firstItemId, 10));
        r1.IsValid.ShouldBeTrue();

        var r2 = service.CancelItem(new CancelItemCommand(sale.Id, firstItemId));
        r2.IsValid.ShouldBeTrue();

        sut.Repo.Received(2).Update(sale);
        
        sut.ShouldPublishEvents(2);
    }

    [Fact]
    public void CancelSale_updates_and_publishes()
    {
        var sut = new SaleServiceSut();
        var sale = Mothers.AExistingSale(out _);
        sut.Query.GetById(sale.Id).Returns(sale);
        var service = sut.Build();

        var res = service.CancelSale(new CancelSaleCommand(sale.Id, "user_request"));
        res.IsValid.ShouldBeTrue();

        sut.Repo.Received(1).Update(sale);
        sut.ShouldPublishEventsOnce();
    }
}
