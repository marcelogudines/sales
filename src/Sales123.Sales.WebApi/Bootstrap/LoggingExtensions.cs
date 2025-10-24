using Serilog;
using Serilog.Events;

namespace Sales123.Sales.WebApi.Bootstrap;

public static class LoggingExtensions
{
    public static void AddMinimalLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Fatal)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Fatal)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Fatal)
            .MinimumLevel.Override("System", LogEventLevel.Fatal)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Fatal)
            .MinimumLevel.Override("MassTransit", LogEventLevel.Fatal)
            .MinimumLevel.Override("Swashbuckle", LogEventLevel.Fatal) 
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}
