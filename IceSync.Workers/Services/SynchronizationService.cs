using IceSync.Domain.Contracts.Managers;
using IceSync.Domain.Contracts.Services;
using IceSync.Workers.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IceSync.Workers.Services;

public class SynchronizationService : BackgroundService, ISynchronizationService
{
    private readonly ILogger<SynchronizationService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly SynchronizationWorkerOptions _options;

    public SynchronizationService(
        ILogger<SynchronizationService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<SynchronizationWorkerOptions> options)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _options = options.Value;
    }


    public async Task SynchronizeWorkflowsAsync()
    {
        _logger.LogInformation("Synchronization started at: {time}", DateTimeOffset.Now);

        using(var scope = _serviceScopeFactory.CreateScope())
        {
            var workflowManager = scope.ServiceProvider.GetRequiredService<IWorkflowManager>();
            try
            {
                await workflowManager.SynchronizeWorkflowsAsync();
                _logger.LogInformation("Synchronization completed successfully at: {time}", DateTimeOffset.Now);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the synchronization process.");
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(_options.IntervalSeconds);
        _logger.LogInformation("Synchronization Service is starting. Interval: {interval} minutes.", interval.TotalMinutes);

        using PeriodicTimer timer = new PeriodicTimer(interval);

        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SynchronizeWorkflowsAsync();
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch(OperationCanceledException)
            {
                _logger.LogInformation("Synchronization Service is stopping.");
                break;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred during periodic synchronization.");
            }
        }

        _logger.LogInformation("Synchronization Service is stopping.");
    }
}