using NSubstitute;
using Sales123.Sales.Application.Abstractions;
using Sales123.Sales.Application.Services;
using Sales123.Sales.Domain.Abstractions;
using Sales123.Sales.Domain.Aggregates;

namespace Sales123.Sales.Test;

public sealed class SaleServiceSut
{
    public ISaleRepository Repo { get; } = Substitute.For<ISaleRepository>();
    public ISaleQueryRepository Query { get; } = Substitute.For<ISaleQueryRepository>();
    public IDomainEventPublisher Pub { get; } = Substitute.For<IDomainEventPublisher>();

    public SaleService Build() => new(Repo, Query, Pub);

    public void ShouldPublishEventsOnce() =>
        Pub.Received(1).Publish(Arg.Is<IEnumerable<IDomainEvent>>(e => e.Any()));

    public void ShouldNotPublish() =>
        Pub.DidNotReceive().Publish(Arg.Any<IEnumerable<IDomainEvent>>());

    public void ShouldPublishEvents(int times) =>
        Pub.Received(times).Publish(Arg.Is<IEnumerable<IDomainEvent>>(e => e.Any()));
}
