using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.DomainEvents;

namespace SettlersOfCrutan.Application.Games.DomainEventHandlers;

public sealed class DiscardHalfCompleteDomainEventHandler(IRealtimePublisher realtimePublisher, IGameRepository gameRepository, ILogger<DiscardHalfCompleteDomainEventHandler> logger)
    : IDomainEventHandler<DiscardHalfCompleteDomainEvent>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly ILogger<DiscardHalfCompleteDomainEventHandler> _logger = logger;

    public async Task<Result<DiscardHalfCompleteDomainEvent>> HandleAsync(DiscardHalfCompleteDomainEvent domainEvent, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(domainEvent.GameId, ct);
        if (game is null) return Result.Failure<DiscardHalfCompleteDomainEvent>(DomainError.NotFound);

        IReadOnlyList<string> recipients = [.. game.Players.Select(p => p.Id.ToString())];

        var message = new DiscardHalfCompleteMessage(domainEvent.GameId.Value, game.CurrentPlayerId().Value);
        await _realtimePublisher.ToGameUsersAsync(domainEvent.GameId, recipients, nameof(DiscardHalfCompleteDomainEvent), message, ct);

        _logger.LogInformation("Game {GameId}: all players have discarded, robber resolution begins for player {PlayerId}",
            domainEvent.GameId.Value, game.CurrentPlayerId().Value);

        return Result.Success(domainEvent);
    }
}

public record DiscardHalfCompleteMessage(Guid GameId, string ActivePlayerId);
