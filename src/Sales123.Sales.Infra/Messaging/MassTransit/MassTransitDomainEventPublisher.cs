
using MassTransit;
using Sales123.Sales.Application.Abstractions;
using Sales123.Sales.Domain.Abstractions;
using Sales123.Sales.Domain.Events;

namespace Sales123.Sales.Infra.Messaging.MassTransit;

public sealed class MassTransitDomainEventPublisher : IDomainEventPublisher
{
    private readonly IBus _bus;
    public MassTransitDomainEventPublisher(IBus bus) => _bus = bus;

    public void Publish(IEnumerable<IDomainEvent> eventsToPublish)
    {
        foreach (var e in eventsToPublish ?? Enumerable.Empty<IDomainEvent>())
        {
            switch (e)
            {
                case SaleCreated x: _bus.Publish(new SaleCreatedMessage(x.SaleId, x.Number, x.OccurredAt)); break;
                case SaleUpdated x: _bus.Publish(new SaleUpdatedMessage(x.SaleId, x.Number, x.OccurredAt)); break;
                case SaleCanceled x: _bus.Publish(new SaleCanceledMessage(x.SaleId, x.Number, x.Reason, x.OccurredAt)); break;
                case SaleItemCanceled x: _bus.Publish(new SaleItemCanceledMessage(x.SaleId, x.Number, x.ItemId, x.OccurredAt)); break;
            }
        }
    }
}
