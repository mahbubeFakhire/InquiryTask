using Domain.Common;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WebApi.Server.Middleware;

public class ServiceLocatorInitilizeMiddleware
{
    private readonly RequestDelegate _next;

    public ServiceLocatorInitilizeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        ServiceLocator.Initialize(httpContext.RequestServices);
        await _next(httpContext);
    }
}
