
namespace Sales123.Sales.Domain.Abstractions;

public interface IDomainEvent
{
    System.DateTimeOffset OccurredAt { get; }
}
