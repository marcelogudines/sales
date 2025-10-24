using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Sales123.Sales.WebApi.Support;
using Sales123.Sales.WebApi.Support.Swagger;

namespace Sales123.Sales.WebApi.Bootstrap;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    => app.UseMiddleware<GlobalExceptionMiddleware>();

    public static IApplicationBuilder UseRequestBuffering(this IApplicationBuilder app)
    => app.UseMiddleware<RequestBufferingMiddleware>();
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();

    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestResponseLoggingMiddleware>();

    public static IApplicationBuilder UseSalesSwaggerUi(this IApplicationBuilder app)
    {
        var ex = app.ApplicationServices.GetRequiredService<ISalesSwaggerExampleProvider>();

        app.UseSwagger(c =>
        {
            c.PreSerializeFilters.Add((doc, httpReq) =>
            {
              
                foreach (var (_, pathItem) in doc.Paths)
                    foreach (var (_, op) in pathItem.Operations)
                    {
                        if (op.Parameters == null) continue;
                        foreach (var p in op.Parameters)
                        {
                            if (p.In == ParameterLocation.Header &&
                                string.Equals(p.Name, "x-correlation-id", StringComparison.OrdinalIgnoreCase))
                            {
                                p.Example = new OpenApiString(ex.NewCorrelationId());
                            }
                        }
                    }

           
                if (doc.Paths.TryGetValue("/api/sales", out var salesRoot) &&
                    salesRoot.Operations.TryGetValue(OperationType.Post, out var post))
                {
                    if (post.RequestBody?.Content?.TryGetValue("application/json", out var mediaReq) == true)
                        mediaReq.Example = ex.BuildCreateRequestExample();

                    if (post.Responses.TryGetValue("201", out var r201) &&
                        r201.Content?.TryGetValue("application/json", out var media201) == true)
                        media201.Example = ex.BuildSaleEnvelopeExample(status: "Created");

                    if (post.Responses.TryGetValue("422", out var r422) &&
                        r422.Content?.TryGetValue("application/json", out var media422) == true)
                        media422.Example = ex.BuildValidation422EnvelopeExample();
                }

              
                if (doc.Paths.TryGetValue("/api/sales", out var salesList) &&
                    salesList.Operations.TryGetValue(OperationType.Get, out var getList))
                {
                    if (getList.Responses.TryGetValue("200", out var r200) &&
                        r200.Content?.TryGetValue("application/json", out var media200) == true)
                        media200.Example = ex.BuildPagedListEnvelopeExample();
                }

              
                if (doc.Paths.TryGetValue("/api/sales/{id}", out var salesById) &&
                    salesById.Operations.TryGetValue(OperationType.Get, out var getById))
                {
                    if (getById.Responses.TryGetValue("200", out var r200Id) &&
                        r200Id.Content?.TryGetValue("application/json", out var media200Id) == true)
                        media200Id.Example = ex.BuildSaleEnvelopeExample(status: "Approved");

                    if (getById.Responses.TryGetValue("404", out var r404Id) &&
                        r404Id.Content?.TryGetValue("application/json", out var media404Id) == true)
                        media404Id.Example = ex.BuildNotFoundEnvelopeExample();
                }

                string[] itemRoutes =
                {
                    "/api/sales/{id}/items",
                    "/api/sales/{id}/items/{itemId}/quantity",
                    "/api/sales/{id}/items/{itemId}/cancel",
                    "/api/sales/{id}/cancel"
                };

                foreach (var route in itemRoutes)
                {
                    if (!doc.Paths.TryGetValue(route, out var pi)) continue;

                    foreach (var (_, op) in pi.Operations)
                    {
                        if (op.Responses.TryGetValue("200", out var rOk) &&
                            rOk.Content?.TryGetValue("application/json", out var mediaOk) == true)
                            mediaOk.Example = ex.BuildSaleEnvelopeExample();

                        if (op.Responses.TryGetValue("404", out var rNot) &&
                            rNot.Content?.TryGetValue("application/json", out var mediaNot) == true)
                            mediaNot.Example = ex.BuildNotFoundEnvelopeExample();

                        if (op.Responses.TryGetValue("422", out var rUnp) &&
                            rUnp.Content?.TryGetValue("application/json", out var mediaUnp) == true)
                            mediaUnp.Example = ex.BuildValidation422EnvelopeExample();
                    }
                }
            });
        });

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sales123.Sales.WebApi v1");
            c.RoutePrefix = string.Empty;
        });

        return app;
    }
}
