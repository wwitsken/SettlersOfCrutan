using Quartz;
using StackExchange.Redis;

namespace SettlersOfCrutan.OutboxService;

[DisallowConcurrentExecution]
public sealed class OutboxPollingQuartzJob(IServiceScopeFactory scopeFactory, ILogger<OutboxPollingQuartzJob> logger) : IJob
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<OutboxPollingQuartzJob> _logger = logger;

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDatabase>();
            var processor = scope.ServiceProvider.GetRequiredService<ProcessOutboxMessagesJob>();

            await processor.ExecuteOnce(db, context.CancellationToken);
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            // shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Outbox polling job failed.");
        }
    }
}
