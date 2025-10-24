using Sales123.Sales.WebApi.Bootstrap;

var builder = WebApplication.CreateBuilder(args);

builder.AddMinimalLogging();

builder.Services.AddApiBasics()
                .AddSalesSwagger();

builder.Services.AddSalesInfra(builder.Configuration)
                .AddApplicationServices()
                .AddHttpClientLogging();

builder.Services.AddRouting(o =>
{
    o.LowercaseUrls = true;
    o.LowercaseQueryStrings = true;
});
var app = builder.Build();

app.UseCorrelationId();
app.UseRequestBuffering();        
app.UseGlobalExceptionHandling();    
app.UseRequestResponseLogging();

app.UseSalesSwaggerUi();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
