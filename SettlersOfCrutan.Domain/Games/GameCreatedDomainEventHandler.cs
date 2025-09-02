using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games;

public sealed class GameCreatedDomainEventHandler : IDomainEventHandler<GameCreatedDomainEvent>
{
    private readonly ILogger<GameCreatedDomainEventHandler> _logger;

    public GameCreatedDomainEventHandler(ILogger<GameCreatedDomainEventHandler> logger) => _logger = logger;

    public Task<Result<GameCreatedDomainEvent>> HandleAsync(GameCreatedDomainEvent domainEvent, CancellationToken ct = default)
    {
        _logger.LogInformation("Game created: {GameId} for players: {Players}", domainEvent.gameId.Value, string.Join(",", domainEvent.playerUserIds));
        return Task.FromResult(Result.Success(domainEvent));
    }
}
