using Sales123.Sales.Application.DTOs;
using Sales123.Sales.Domain.Aggregates;
using Sales123.Sales.Domain.Entities;
using Sales123.Sales.Domain.ValueObjects;

namespace Sales123.Sales.Test;

public static class Mothers
{
    public static CreateSaleCommand ACreateSaleCommand() =>
        new CreateSaleCommand(
            Number: "N-1",
            SaleDate: DateTimeOffset.UtcNow,
            CustomerId: "C1",
            CustomerName: "Cliente 1",
            BranchId: "BR-01",
            BranchName: "Filial 01",
            Items: new[] { new CreateSaleItemDto("P1", "Produto 1", "SKU-1", 1, 10m) });

    public static AddItemCommand AnAddItem(string saleId) =>
        new AddItemCommand(saleId, "P2", "Produto 2", "SKU-2", 5, 10m);

    public static ReplaceItemQuantityCommand AReplaceQty(string saleId, string itemId, int qty) =>
        new ReplaceItemQuantityCommand(saleId, itemId, qty);

    public static CancelSaleCommand ACancelSale(string saleId, string? reason = "motivo") =>
        new CancelSaleCommand(saleId, reason);

  
    public static Sale AExistingSale(out string firstItemId)
    {
        var cust = CustomerRef.Create("C1", "Cliente 1").Value!;
        var br = BranchRef.Create("BR-01", "Filial 01").Value!;
        var input = new SaleItemInput(
            ProductRef.Create("P1", "Produto 1", "SKU-1").Value!,
            1,
            Money.FromRaw(10m));

        var sale = Sale.Create("N-1", DateTimeOffset.UtcNow, cust, br, new[] { input }).Value!;
        sale.DequeueEvents(); 
        firstItemId = sale.Items.First().Id;
        return sale;
    }
}
