using Sales123.Sales.WebApi.Bootstrap;
using Sales123.Sales.WebApi.Support;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.AddMinimalLogging();

builder.Services.AddApiBasics()
                .AddSalesSwagger();

builder.Services.AddSalesInfra(builder.Configuration)
                .AddApplicationServices()
                .AddHttpClientLogging();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("OK"));

builder.Services.AddSingleton<RequestMetrics>();
builder.Services.AddHostedService<MetricsSummaryHostedService>();

builder.Services.AddRouting(o =>
{
    o.LowercaseUrls = true;
    o.LowercaseQueryStrings = true;
});

var app = builder.Build();

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
var health = app.Services.GetRequiredService<HealthCheckService>();

lifetime.ApplicationStarted.Register(() =>
{
    startupLogger.LogInformation("Application started | env={Env} | version={Version} | urls={Urls}",
        app.Environment.EnvironmentName,
        typeof(Program).Assembly.GetName().Version?.ToString(),
        Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "(default)");

    _ = Task.Run(async () =>
    {
        try
        {
            var report = await health.CheckHealthAsync(_ => true);
            startupLogger.LogInformation(
                "HealthCheck initial status={Status} durationMs={Duration} entries={Entries}",
                report.Status.ToString(),
                report.TotalDuration.TotalMilliseconds,
                string.Join(", ", report.Entries.Select(e => $"{e.Key}:{e.Value.Status}")));
        }
        catch (Exception ex)
        {
            startupLogger.LogError(ex, "HealthCheck initial run failed");
        }
    });
});

lifetime.ApplicationStopping.Register(() => startupLogger.LogInformation("Application stopping"));
lifetime.ApplicationStopped.Register(() => startupLogger.LogInformation("Application stopped"));

app.UseCorrelationId();
app.UseRequestBuffering();
app.UseGlobalExceptionHandling();
app.UseRequestResponseLogging();

app.UseSalesSwaggerUi();

app.MapHealthChecks("/health"); // compat
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = r => r.Name == "self" });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = _ => true });

app.MapGet("/metrics-app", (RequestMetrics m) => Results.Json(m.Snapshot()));

app.MapControllers();

app.Run();
