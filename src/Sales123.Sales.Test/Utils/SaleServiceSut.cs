using Moq;
using Sales123.Sales.Application.Abstractions;
using Sales123.Sales.Application.Services;
using Sales123.Sales.Domain.Abstractions;
using Sales123.Sales.Domain.Aggregates;
using Sales123.Sales.Domain.Events;

namespace Sales123.Sales.Test;

public sealed class SaleServiceSut
{
    public Mock<ISaleRepository> Repo { get; } = new();
    public Mock<ISaleQueryRepository> Query { get; } = new();
    public Mock<IDomainEventPublisher> Pub { get; } = new();

    public SaleService Build() => new(Repo.Object, Query.Object, Pub.Object);

    public void ShouldPublishEventsOnce() =>
        Pub.Verify(p => p.Publish(It.Is<IEnumerable<IDomainEvent>>(e => e.Any())), Times.Once);

    public void ShouldNotPublish() =>
        Pub.Verify(p => p.Publish(It.IsAny<IEnumerable<IDomainEvent>>()), Times.Never);
}
