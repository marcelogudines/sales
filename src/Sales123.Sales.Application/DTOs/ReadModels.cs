
using Sales123.Sales.Domain.Aggregates;

namespace Sales123.Sales.Application.DTOs;

public sealed record SaleItemView(string Id, string ProductId, string ProductName, string? Sku, int Quantity, int DiscountPercent, decimal UnitPrice, decimal ItemTotal, bool Canceled);
public sealed record SaleView(string Id, string Number, DateTimeOffset SaleDate, string CustomerId, string CustomerName, string BranchId, string BranchName, string Status, decimal SaleTotal, IEnumerable<SaleItemView> Items);

public static class SaleViewMapper
{
    public static SaleView ToView(Sale s) => new(
        s.Id, s.Number, s.SaleDate,
        s.Customer.Id, s.Customer.Name,
        s.Branch.Id, s.Branch.Name,
        s.Status.ToString(),
        s.SaleTotal.Value,
        s.Items.Select(i => new SaleItemView(i.Id, i.Product.Id, i.Product.Name, i.Product.Sku, i.Quantity, i.DiscountPercent, i.UnitPrice.Value, i.ItemTotal.Value, i.Canceled))
    );
}
