
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi.Server.Filters;

public sealed class HeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Client-Idempotency-Key",
            In = ParameterLocation.Header,
            Description = "Idempotency Key",
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = "string"
            }
        });
    }

}
