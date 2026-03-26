using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.DomainEvents;

namespace SettlersOfCrutan.Application.Games.DomainEventHandlers;

public sealed class PlayerDiscardedHalfDomainEventHandler(IRealtimePublisher realtimePublisher, IGameRepository gameRepository, ILogger<PlayerDiscardedHalfDomainEventHandler> logger)
    : IDomainEventHandler<PlayerDiscardedHalfDomainEvent>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly ILogger<PlayerDiscardedHalfDomainEventHandler> _logger = logger;

    public async Task<Result<PlayerDiscardedHalfDomainEvent>> HandleAsync(PlayerDiscardedHalfDomainEvent domainEvent, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(domainEvent.GameId, ct);
        if (game is null) return Result.Failure<PlayerDiscardedHalfDomainEvent>(DomainError.NotFound);

        IReadOnlyList<string> allRecipients = [.. game.Players.Select(p => p.Id.ToString())];

        int totalDiscarded = domainEvent.ResourceAmounts.Sum(r => r.Quantity);
        var remainingDiscardPlayerIds = game.DiscardHalfRequirements.Select(r => r.PlayerId.Value).ToList();

        var message = new PlayerDiscardedHalfMessage(
            domainEvent.GameId.Value,
            domainEvent.PlayerId.Value,
            totalDiscarded,
            remainingDiscardPlayerIds);

        await _realtimePublisher.ToGameUsersAsync(domainEvent.GameId, allRecipients, nameof(PlayerDiscardedHalfDomainEvent), message, ct);

        _logger.LogInformation("Game {GameId}: player {PlayerId} discarded {Count} cards",
            domainEvent.GameId.Value, domainEvent.PlayerId.Value, totalDiscarded);

        return Result.Success(domainEvent);
    }
}

public record PlayerDiscardedHalfMessage(Guid GameId, string PlayerId, int TotalDiscarded, List<string> RemainingDiscardPlayerIds);
