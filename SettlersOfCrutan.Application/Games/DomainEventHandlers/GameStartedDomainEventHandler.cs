using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.DomainEvents;

namespace SettlersOfCrutan.Application.Games.DomainEventHandlers;

public sealed class GameStartedDomainEventHandler(IRealtimePublisher realtimePublisher, IGameRepository gameRepository, ILogger<GameStartedDomainEventHandler> logger)
    : IDomainEventHandler<GameStartedDomainEvent>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly ILogger<GameStartedDomainEventHandler> _logger = logger;

    public async Task<Result<GameStartedDomainEvent>> HandleAsync(GameStartedDomainEvent domainEvent, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(domainEvent.GameId, ct);
        if (game is null) return Result.Failure<GameStartedDomainEvent>(DomainError.NotFound);

        IReadOnlyList<string> recipients = [.. game.Players.Select(p => p.Id.ToString())];

        var message = new GameStartedMessage(
            domainEvent.GameId.Value,
            domainEvent.StartingPlayerId.Value,
            game.GamePhase.ToString());

        await _realtimePublisher.ToGameUsersAsync(domainEvent.GameId, recipients, nameof(GameStartedDomainEvent), message, ct);

        _logger.LogInformation("Game {GameId} started. First turn: player {PlayerId}", domainEvent.GameId.Value, domainEvent.StartingPlayerId.Value);

        return Result.Success(domainEvent);
    }
}

public record GameStartedMessage(Guid GameId, string StartingPlayerId, string GamePhase);
