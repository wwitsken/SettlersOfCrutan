using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.DomainEvents;

namespace SettlersOfCrutan.Application.Games.DomainEventHandlers;

public sealed class TradeBuildStartedDomainEventHandler(IRealtimePublisher realtimePublisher, IGameRepository gameRepository, ILogger<TradeBuildStartedDomainEventHandler> logger)
    : IDomainEventHandler<TradeBuildStartedDomainEvent>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly ILogger<TradeBuildStartedDomainEventHandler> _logger = logger;

    public async Task<Result<TradeBuildStartedDomainEvent>> HandleAsync(TradeBuildStartedDomainEvent domainEvent, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(domainEvent.GameId, ct);
        if (game is null) return Result.Failure<TradeBuildStartedDomainEvent>(DomainError.NotFound);

        IReadOnlyList<string> recipients = [.. game.Players.Select(p => p.Id.ToString())];

        var message = new TradeBuildStartedMessage(domainEvent.GameId.Value, domainEvent.PlayerId.Value);
        await _realtimePublisher.ToGameUsersAsync(domainEvent.GameId, recipients, nameof(TradeBuildStartedDomainEvent), message, ct);

        _logger.LogInformation("Game {GameId}: trade and build phase started for player {PlayerId}",
            domainEvent.GameId.Value, domainEvent.PlayerId.Value);

        return Result.Success(domainEvent);
    }
}

public record TradeBuildStartedMessage(Guid GameId, string ActivePlayerId);
