using Domain.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace WebApi.Server.Services;

public class ClientHeaders : IClientHeaders
{
    private readonly IHttpContextAccessor _contextAccessor;
    public ClientHeaders(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public string ClientIdempotencyKey => GetHeader("X-Client-Idempotency-Key") ?? "";

    public string GetHeader(string headerName)
    {
        try
        {
            var request = _contextAccessor?.HttpContext?.Request;
            return request?.Headers[headerName];
        }
        catch
        {
            return null;
        }

    }
}
