using Bogus;
using Sales123.Sales.Application.DTOs;
using Sales123.Sales.Domain.Aggregates;
using Sales123.Sales.Domain.Entities;
using Sales123.Sales.Domain.ValueObjects;

namespace Sales123.Sales.Test;

public static class Mothers
{
    private static readonly Faker Faker = new("pt_BR");

    public static CreateSaleCommand ACreateSaleCommand(int items = 1)
    {
        var custId = Faker.Random.Hash(8);
        var brId = Faker.PickRandom(new []{"BR-01","BR-02"});
        var number = $"S-{DateTime.UtcNow:yyyyMMdd}-{Faker.Random.Int(1,99999):D5}";

        var list = Enumerable.Range(0, items).Select(i =>
            new CreateSaleItemDto(
                ProductId: $"P{Faker.Random.Int(1,5000)}",
                ProductName: Faker.Commerce.ProductName(),
                Sku: Faker.Commerce.Ean8(),
                Quantity: Faker.Random.Int(1, 12),
                UnitPrice: Faker.Random.Decimal(10, 500))
        );

        return new CreateSaleCommand(
            Number: number,
            SaleDate: DateTimeOffset.UtcNow,
            CustomerId: custId,
            CustomerName: Faker.Company.CompanyName(),
            BranchId: brId,
            BranchName: Faker.Address.City(),
            Items: list.ToList());
    }

    public static AddItemCommand AnAddItem(string saleId)
        => new AddItemCommand(
            SaleId: saleId,
            ProductId: $"P{Faker.Random.Int(1,9999)}",
            ProductName: Faker.Commerce.ProductName(),
            Sku: Faker.Commerce.Ean8(),
            Quantity: Faker.Random.Int(1, 12),
            UnitPrice: Faker.Random.Decimal(5, 300));

    public static ReplaceItemQuantityCommand AReplaceQty(string saleId, string itemId, int newQty)
        => new ReplaceItemQuantityCommand(saleId, itemId, newQty);

    // compat com seus testes existentes
    public static Sale AExistingSale(out string firstItemId)
    {
        var cust = CustomerRef.Create("C1", "Cliente 1").Value!;
        var br = BranchRef.Create("BR-01", "Filial 01").Value!;
        var input = new SaleItemInput(
            ProductRef.Create("P1", "Produto 1", "SKU-1").Value!,
            1,
            Money.FromRaw(10m));

        var sale = Sale.Create("N-1", DateTimeOffset.UtcNow, cust, br, new[] { input }).Value!;
        sale.DequeueEvents(); // drena eventos iniciais
        firstItemId = sale.Items.First().Id;
        return sale;
    }
}
