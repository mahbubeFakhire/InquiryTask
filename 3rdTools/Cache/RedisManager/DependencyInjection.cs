using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace RedisManager;

public static class DependencyInjection
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options => {
            configuration.GetSection("Redis").Bind(options);
        });

        return services;
    }
}
