using Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApi.Server.Services;

public class HostRunner : IHostedService
{
    private readonly CancellationTokenSource _stopTokenSource = new();

    private readonly IServiceProvider _serviceProvider;
    private IServiceScope _scopedServiceProvider;


    public HostRunner(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {

        try
        {
            _scopedServiceProvider = _serviceProvider.CreateScope();
            ServiceLocator.Initialize(_scopedServiceProvider.ServiceProvider);
        }
        catch (Exception)
        {
            throw;
        }

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _stopTokenSource.Cancel();
    }
}
