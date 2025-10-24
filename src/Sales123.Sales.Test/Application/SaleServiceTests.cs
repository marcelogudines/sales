using FluentAssertions;
using Moq;
using Sales123.Sales.Domain.Aggregates;
using Xunit;

namespace Sales123.Sales.Test;

public class SaleServiceTests
{

    [Fact]
    public void Create_persists_and_publishes_when_new()
    {
        var sut = new SaleServiceSut();
        sut.Repo.Setup(r => r.AddIfNotExists(It.IsAny<Sale>()))
                .Returns((Sale s) => (true, s));
        var service = sut.Build();
        var cmd = Mothers.ACreateSaleCommand();

        var res = service.Create(cmd);

        res.IsValid.Should().BeTrue();
        sut.Repo.Verify(r => r.AddIfNotExists(It.IsAny<Sale>()), Times.Once);
        sut.ShouldPublishEventsOnce();
    }
    [Fact]
    public void Create_returns_conflict_notification_when_already_exists()
    {
      
        var sut = new SaleServiceSut();
        var existing = Mothers.AExistingSale(out _);
        sut.Repo.Setup(r => r.AddIfNotExists(It.IsAny<Sale>()))
                .Returns((Sale _) => (false, existing));
        var service = sut.Build();

      
        var res = service.Create(Mothers.ACreateSaleCommand());

        res.IsValid.Should().BeFalse();
        res.ShouldHaveNotification("sale.already_exists");
        sut.ShouldNotPublish();
    }

    [Fact]
    public void Create_fails_when_header_invalid()
    {
        var invalid = new Application.DTOs.CreateSaleCommand(
            Number: "N-1",
            SaleDate: DateTimeOffset.UtcNow,
            CustomerId: null!,                 
            CustomerName: "Cliente 1",
            BranchId: "BR-01",
            BranchName: "Filial 01",
            Items: new[] { new Application.DTOs.CreateSaleItemDto("P1", "Produto 1", "SKU-1", 1, 10m) });

        var sut = new SaleServiceSut();
        var service = sut.Build();

        var res = service.Create(invalid);

      
        res.IsValid.Should().BeFalse();
        sut.Repo.Verify(r => r.AddIfNotExists(It.IsAny<Sale>()), Times.Never);
        sut.ShouldNotPublish();
    }


    [Fact]
    public void AddItem_fails_when_sale_not_found()
    {
        var sut = new SaleServiceSut();
        sut.Query.Setup(q => q.GetById(It.IsAny<string>())).Returns((Sale)null!);
        var service = sut.Build();

        var res = service.AddItem(Mothers.AnAddItem("S-404"));

        res.IsValid.Should().BeFalse();
        res.ShouldHaveNotification("sale.not_found");
        sut.Repo.Verify(r => r.Update(It.IsAny<Sale>()), Times.Never);
        sut.ShouldNotPublish();
    }

    [Fact]
    public void AddItem_updates_and_publishes()
    {
        var sut = new SaleServiceSut();
        var sale = Mothers.AExistingSale(out _);
        sut.Query.Setup(q => q.GetById(sale.Id)).Returns(sale);
        var service = sut.Build();

        var res = service.AddItem(Mothers.AnAddItem(sale.Id));

        res.IsValid.Should().BeTrue();
        sut.Repo.Verify(r => r.Update(sale), Times.Once);
        sut.ShouldPublishEventsOnce();
    }

    [Fact]
    public void ReplaceItemQuantity_fails_when_sale_not_found()
    {
        var sut = new SaleServiceSut();
        sut.Query.Setup(q => q.GetById(It.IsAny<string>())).Returns((Sale)null!);
        var service = sut.Build();

        var res = service.ReplaceItemQuantity(Mothers.AReplaceQty("S-404", "I", 10));

        res.IsValid.Should().BeFalse();
        res.ShouldHaveNotification("sale.not_found");
        sut.Repo.Verify(r => r.Update(It.IsAny<Sale>()), Times.Never);
        sut.ShouldNotPublish();
    }

    [Fact]
    public void ReplaceItemQuantity_updates_and_publishes()
    {
        var sut = new SaleServiceSut();
        var sale = Mothers.AExistingSale(out var itemId);
        sut.Query.Setup(q => q.GetById(sale.Id)).Returns(sale);
        var service = sut.Build();

        var res = service.ReplaceItemQuantity(Mothers.AReplaceQty(sale.Id, itemId, 10));

        res.IsValid.Should().BeTrue();
        sale.Items.First(i => i.Id == itemId).Quantity.Should().Be(10);
        sut.Repo.Verify(r => r.Update(sale), Times.Once);
        sut.ShouldPublishEventsOnce();
    }

    [Fact]
    public void CancelItem_fails_when_sale_not_found()
    {
        var sut = new SaleServiceSut();
        sut.Query.Setup(q => q.GetById(It.IsAny<string>())).Returns((Sale)null!);
        var service = sut.Build();

        var res = service.CancelItem(new Application.DTOs.CancelItemCommand("S-404", "I"));

        res.IsValid.Should().BeFalse();
        res.ShouldHaveNotification("sale.not_found");
        sut.Repo.Verify(r => r.Update(It.IsAny<Sale>()), Times.Never);
        sut.ShouldNotPublish();
    }

    [Fact]
    public void CancelItem_updates_and_publishes()
    {
        var sut = new SaleServiceSut();
        var sale = Mothers.AExistingSale(out var firstItemId);
        sut.Query.Setup(q => q.GetById(sale.Id)).Returns(sale);
        var service = sut.Build();

        var res = service.CancelItem(new Application.DTOs.CancelItemCommand(sale.Id, firstItemId));

        res.IsValid.Should().BeTrue();
        sale.Items.First(i => i.Id == firstItemId).Canceled.Should().BeTrue();
        sut.Repo.Verify(r => r.Update(sale), Times.Once);
        sut.ShouldPublishEventsOnce();
    }

  
    [Fact]
    public void CancelSale_fails_when_sale_not_found()
    {
        var sut = new SaleServiceSut();
        sut.Query.Setup(q => q.GetById(It.IsAny<string>())).Returns((Sale)null!);
        var service = sut.Build();

        var res = service.CancelSale(Mothers.ACancelSale("S-404"));

        res.IsValid.Should().BeFalse();
        res.ShouldHaveNotification("sale.not_found");
        sut.Repo.Verify(r => r.Update(It.IsAny<Sale>()), Times.Never);
        sut.ShouldNotPublish();
    }

    [Fact]
    public void CancelSale_updates_and_publishes()
    {
        var sut = new SaleServiceSut();
        var sale = Mothers.AExistingSale(out _);
        sut.Query.Setup(q => q.GetById(sale.Id)).Returns(sale);
        var service = sut.Build();

        var res = service.CancelSale(Mothers.ACancelSale(sale.Id, "motivo"));

        res.IsValid.Should().BeTrue();
        sut.Repo.Verify(r => r.Update(sale), Times.Once);
        sut.ShouldPublishEventsOnce();
    }

    [Fact]
    public void Delete_delegates_to_repository()
    {
        var sut = new SaleServiceSut();
        sut.Repo.Setup(r => r.Delete("X")).Returns(true);
        var service = sut.Build();

        service.Delete("X").Should().BeTrue();
        sut.Repo.Verify(r => r.Delete("X"), Times.Once);
    }
}
