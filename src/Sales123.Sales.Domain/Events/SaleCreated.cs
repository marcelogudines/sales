
using Sales123.Sales.Domain.Abstractions;

namespace Sales123.Sales.Domain.Events;

public sealed record SaleCreated(string SaleId, string Number) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
