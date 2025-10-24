
using Sales123.Sales.Domain.Abstractions;

namespace Sales123.Sales.Application.Abstractions;

public interface IDomainEventPublisher
{
    void Publish(IEnumerable<IDomainEvent> eventsToPublish);
}
