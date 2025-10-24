using Sales123.Sales.Domain.ValueObjects;
using Sales123.Sales.Domain.Entities;

namespace Sales123.Sales.Test.Utils;

public static class TestHelper
{
    public static ProductRef Product(string? id = null, string? name = null, string? sku = "SKU-1")
        => ProductRef.Create(id ?? Guid.NewGuid().ToString("N"), name ?? "Produto X", sku).Value!;

    public static CustomerRef Customer(string? id = null, string? name = null)
        => CustomerRef.Create(id ?? Guid.NewGuid().ToString("N"), name ?? "Cliente X").Value!;

    public static BranchRef Branch(string? id = null, string? name = null)
        => BranchRef.Create(id ?? "BR-01", name ?? "Filial 01").Value!;

    public static Money Money(decimal value) => Sales.Domain.ValueObjects.Money.From(value).Value!;

    public static SaleItemInput ItemInput(int qty = 1, decimal unit = 10m)
        => new SaleItemInput(Product(), qty, Money(unit));
}
