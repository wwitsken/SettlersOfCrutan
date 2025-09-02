using StackExchange.Redis;
using System.Diagnostics;

namespace SettlersOfCrutan.OutboxService;

// Hosted background service that runs the outbox processor loop.
public sealed class OutboxBackgroundService(IServiceScopeFactory scopeFactory, ILogger<OutboxBackgroundService> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<OutboxBackgroundService> _logger = logger;

    public const string ActivitySourceName = "Outbox";
    private static readonly ActivitySource _activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = _activitySource.StartActivity("Processing outbox messages", ActivityKind.Server);

        _logger.LogInformation("OutboxBackgroundService starting");
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDatabase>();
            var job = scope.ServiceProvider.GetRequiredService<ProcessOutboxMessagesJob>();

            await job.Execute(db);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // graceful shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OutboxBackgroundService encountered an unhandled exception.");
            activity?.AddException(ex);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // small delay to avoid tight crash loop
        }
        finally
        {
            _logger.LogInformation("OutboxBackgroundService stopping");
        }
    }
}
