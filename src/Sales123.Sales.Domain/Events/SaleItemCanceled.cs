
using Sales123.Sales.Domain.Abstractions;

namespace Sales123.Sales.Domain.Events;

public sealed record SaleItemCanceled(string SaleId, string Number, string ItemId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
