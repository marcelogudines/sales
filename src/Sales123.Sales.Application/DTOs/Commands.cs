
namespace Sales123.Sales.Application.DTOs;

public sealed record CreateSaleItemDto(string ProductId, string ProductName, string? Sku, int Quantity, decimal UnitPrice);
public sealed record CreateSaleCommand(
    string Number,
    DateTimeOffset SaleDate,
    string CustomerId,
    string CustomerName,
    string BranchId,
    string BranchName,
    IEnumerable<CreateSaleItemDto> Items);

public sealed record AddItemCommand(string SaleId, string ProductId, string ProductName, string? Sku, int Quantity, decimal UnitPrice);
public sealed record ReplaceItemQuantityCommand(string SaleId, string ItemId, int NewQuantity);
public sealed record CancelItemCommand(string SaleId, string ItemId);
public sealed record CancelSaleCommand(string SaleId, string? Reason);
