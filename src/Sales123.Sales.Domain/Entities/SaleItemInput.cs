
using Sales123.Sales.Domain.ValueObjects;

namespace Sales123.Sales.Domain.Entities;

public sealed record SaleItemInput(ProductRef Product, int Quantity, Money UnitPrice);
