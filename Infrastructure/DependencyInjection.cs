using Domain.AppSettings;
using Domain.InquiryRequests;
using Extensions;
using Infrastructure.ExternalServices.Inquiries;
using Infrastructure.ExternalServices.Inquiries.Models;
using Infrastructure.InquiryRequests;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RedisManager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextFactory<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddRedis(configuration);

        services.AddMemoryCache();
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddTransient<IInquiryRequestRepository, InquiryRequestRepository>();

        services.Configure<InquiryRequestSetting>(options => configuration.GetSection(InquiryRequestSetting.SectionName).Bind(options));

        services.AddSingleton<IInquiryProvider, ProviderA>();
        services.AddSingleton<IInquiryProvider, ProviderB>();

        services.AddSingleton<IReadOnlyList<InquiryProviderRoute>>(provider =>
        {
            var registered = provider.GetServices<IInquiryProvider>().ToDictionary(item => item.Key, StringComparer.Ordinal);
            var configured = provider.GetRequiredService<IOptions<InquiryRequestSetting>>().Value.Providers;

            var routes = new List<InquiryProviderRoute>();
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in configured)
            {
                var key = entry.Key;
                var timeout = entry.Timeout;
                if (key.IsNullOrWhiteSpace() || registered.TryGetValue(key, out var inquiryProvider).Not())
                    throw new InvalidOperationException($"Unknown inquiry provider '{key}'.");

                if (!keys.Add(key))
                    throw new InvalidOperationException($"Duplicate inquiry provider '{key}'.");

                if (timeout <= TimeSpan.Zero || timeout > TimeSpan.FromDays(1))
                    throw new InvalidOperationException($"Invalid timeout for inquiry provider '{key}'.");

                routes.Add(new InquiryProviderRoute(inquiryProvider, timeout));
            }

            if (routes.Count < 2)
                throw new InvalidOperationException("Configure at least two inquiry providers.");

            return routes.AsReadOnly();
        });

        return services;
    }
}
