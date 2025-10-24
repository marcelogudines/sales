using MassTransit;
using Sales123.Sales.Application.Abstractions;
using Sales123.Sales.Application.Services;
using Sales123.Sales.Infra.Messaging.MassTransit;
using Sales123.Sales.Infra.Persistence.InMemory;

namespace Sales123.Sales.WebApi.Bootstrap;

public static class InfraExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ISaleService, SaleService>();
        return services;
    }

    public static IServiceCollection AddSalesInfra(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddSingleton<InMemorySaleRepository>();
        services.AddSingleton<ISaleRepository>(sp => sp.GetRequiredService<InMemorySaleRepository>());
        services.AddSingleton<ISaleQueryRepository>(sp => sp.GetRequiredService<InMemorySaleRepository>());

        services.AddMassTransit(x =>
        {
            x.AddConsumer<SaleCreatedConsumer>();
            x.AddConsumer<SaleUpdatedConsumer>();
            x.AddConsumer<SaleCanceledConsumer>();
            x.AddConsumer<SaleItemCanceledConsumer>();
            x.AddConsumer<ApiErrorOccurredConsumer>();
            x.UsingInMemory((ctx, cfgBus) => cfgBus.ConfigureEndpoints(ctx));
        });

        services.AddScoped<IDomainEventPublisher, MassTransitDomainEventPublisher>();
        return services;

    }
}
