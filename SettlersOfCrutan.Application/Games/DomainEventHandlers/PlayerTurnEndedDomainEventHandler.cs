using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.DomainEvents;

namespace SettlersOfCrutan.Application.Games.DomainEventHandlers;

public sealed class PlayerTurnEndedDomainEventHandler(IRealtimePublisher realtimePublisher, IGameRepository gameRepository, ILogger<PlayerTurnEndedDomainEventHandler> logger)
    : IDomainEventHandler<PlayerTurnEndedDomainEvent>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly ILogger<PlayerTurnEndedDomainEventHandler> _logger = logger;

    public async Task<Result<PlayerTurnEndedDomainEvent>> HandleAsync(PlayerTurnEndedDomainEvent domainEvent, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(domainEvent.GameId, ct);
        if (game is null) return Result.Failure<PlayerTurnEndedDomainEvent>(DomainError.NotFound);

        IReadOnlyList<string> recipients = [.. game.Players.Select(p => p.Id.ToString())];

        var message = new PlayerTurnEndedMessage(
            domainEvent.GameId.Value,
            domainEvent.OldPlayer.Value,
            domainEvent.CurrentPlayer.Value,
            game.GamePhase.ToString(),
            game.Round);

        await _realtimePublisher.ToGameUsersAsync(domainEvent.GameId, recipients, nameof(PlayerTurnEndedDomainEvent), message, ct);

        _logger.LogInformation("Game {GameId}: turn ended for {OldPlayer}, now {CurrentPlayer}'s turn in phase {Phase}",
            domainEvent.GameId.Value, domainEvent.OldPlayer.Value, domainEvent.CurrentPlayer.Value, game.GamePhase);

        return Result.Success(domainEvent);
    }
}

public record PlayerTurnEndedMessage(Guid GameId, string OldPlayerId, string CurrentPlayerId, string GamePhase, int Round);
