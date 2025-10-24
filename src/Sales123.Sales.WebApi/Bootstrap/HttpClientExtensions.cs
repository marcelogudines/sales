using Sales123.Sales.WebApi.Support;

namespace Sales123.Sales.WebApi.Bootstrap;

public static class HttpClientExtensions
{
    public static IServiceCollection AddHttpClientLogging(this IServiceCollection services)
    {
        services.AddTransient<HttpLoggingHandler>();

        services.AddHttpClient("logged")
                .AddHttpMessageHandler<HttpLoggingHandler>();

        return services;
    }
}
