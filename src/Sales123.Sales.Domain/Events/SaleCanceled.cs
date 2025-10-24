
using Sales123.Sales.Domain.Abstractions;

namespace Sales123.Sales.Domain.Events;

public sealed record SaleCanceled(string SaleId, string Number, string? Reason) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
