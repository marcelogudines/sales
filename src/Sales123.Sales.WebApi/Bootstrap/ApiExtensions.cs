using Microsoft.OpenApi.Models;
using Sales123.Sales.WebApi.Support;
using Sales123.Sales.WebApi.Support.Swagger;

namespace Sales123.Sales.WebApi.Bootstrap;

public static class ApiExtensions
{
    public static IServiceCollection AddApiBasics(this IServiceCollection services)
    {
        services.AddControllers()
            .ConfigureApiBehaviorOptions(o => o.SuppressModelStateInvalidFilter = true);

        services.AddEndpointsApiExplorer();
        services.AddHealthChecks();

        services.AddSingleton<RequestMetrics>();

        return services;
    }

    public static IServiceCollection AddSalesSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "123Vendas - Sales API",
                Version = "v1",
                Description = "API de Vendas",
                Contact = new OpenApiContact { Name = "Equipe Vendas 123", Email = "marcelogud@gmail.com" }
            });

            c.SupportNonNullableReferenceTypes();
            c.OperationFilter<CorrelationHeaderOperationFilter>();

            var xml = Path.Combine(AppContext.BaseDirectory, "Sales123.Sales.WebApi.xml");
            if (File.Exists(xml)) c.IncludeXmlComments(xml);
        });

        services.AddSingleton<ISalesSwaggerExampleProvider, SalesSwaggerExampleProvider>();
        return services;
    }
}
