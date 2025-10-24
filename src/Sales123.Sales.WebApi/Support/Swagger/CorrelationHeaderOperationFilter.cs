using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Sales123.Sales.WebApi.Support.Swagger
{
    public sealed class CorrelationHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();
            var exists = operation.Parameters.Any(p =>
                p.In == ParameterLocation.Header &&
                string.Equals(p.Name, "x-correlation-id", StringComparison.OrdinalIgnoreCase));
            if (exists) return;

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "x-correlation-id",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Identificador de correlação (se não enviado, o servidor gera).",
                Schema = new OpenApiSchema { Type = "string" }
            });
        }
    }
}
