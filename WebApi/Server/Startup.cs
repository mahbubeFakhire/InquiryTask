using Application;
using Application.Contracts;
using Domain.Common.Interfaces;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System;
using WebApi.Server.Filters;
using WebApi.Server.Middleware;
using WebApi.Server.Services;
using WebApi.Server.SqlServerConfigProviders;

namespace WebApi.Server;

public class Startup
{
    private const string _allowSpecificOriginsKey = "_allowSpecificOrigins";

    public Startup(IConfiguration configuration)
    {
        var updateConfiguration = new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .AddEntityConfiguration()
            .Build();

        Configuration = updateConfiguration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), Configuration));

        services.AddHttpContextAccessor();

        services.AddApplication();
        services.AddInfrastructure(Configuration);

        services.AddSingleton<IHostedService, HostRunner>();
        services.AddScoped<IClientHeaders, ClientHeaders>();

        services.AddScoped<ApiExceptionFilter>();

        services.AddControllers(options =>
        {
            options.Filters.Add<ApiExceptionFilter>();
        });

        services.AddCors(options =>
        {
            options.AddPolicy(_allowSpecificOriginsKey,
                builder =>
                {
                    builder
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });
        });

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            c.OperationFilter<HeaderOperationFilter>();
        });

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider sp, ILoggerFactory loggerFactory)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
            });
        }
        else
        {
            app.UseHsts();
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseCors(_allowSpecificOriginsKey);

        app.UseMiddleware<ServiceLocatorInitilizeMiddleware>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
