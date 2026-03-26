using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.DomainEvents;

namespace SettlersOfCrutan.Application.Games.DomainEventHandlers;

public sealed class DiscardHalfStartedDomainEventHandler(IRealtimePublisher realtimePublisher, IGameRepository gameRepository, ILogger<DiscardHalfStartedDomainEventHandler> logger)
    : IDomainEventHandler<DiscardHalfStartedDomainEvent>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly ILogger<DiscardHalfStartedDomainEventHandler> _logger = logger;

    public async Task<Result<DiscardHalfStartedDomainEvent>> HandleAsync(DiscardHalfStartedDomainEvent domainEvent, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(domainEvent.GameId, ct);
        if (game is null) return Result.Failure<DiscardHalfStartedDomainEvent>(DomainError.NotFound);

        IReadOnlyList<string> allRecipients = [.. game.Players.Select(p => p.Id.ToString())];
        var requiredPlayerIds = domainEvent.RequiredPlayersToDiscard.Select(p => p.Value).ToList();

        // Notify all players who needs to discard
        var publicMessage = new DiscardHalfStartedMessage(domainEvent.GameId.Value, requiredPlayerIds);
        await _realtimePublisher.ToGameUsersAsync(domainEvent.GameId, allRecipients, nameof(DiscardHalfStartedDomainEvent), publicMessage, ct);

        // Notify each required player privately with their specific discard amount
        foreach (var req in game.DiscardHalfRequirements)
        {
            var privateMessage = new DiscardHalfRequiredMessage(domainEvent.GameId.Value, req.PlayerId.Value, req.ResourceAmount);
            await _realtimePublisher.ToGameUserAsync(domainEvent.GameId, req.PlayerId.ToString(), nameof(DiscardHalfRequiredMessage), privateMessage, ct);
        }

        _logger.LogInformation("Game {GameId}: discard half started for {PlayerCount} players",
            domainEvent.GameId.Value, domainEvent.RequiredPlayersToDiscard.Count);

        return Result.Success(domainEvent);
    }
}

public record DiscardHalfStartedMessage(Guid GameId, List<string> RequiredPlayerIds);
public record DiscardHalfRequiredMessage(Guid GameId, string PlayerId, int AmountToDiscard);
