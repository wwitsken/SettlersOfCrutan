using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.DomainEvents;

namespace SettlersOfCrutan.Application.Games.DomainEventHandlers;

public sealed class DiceRolledDomainEventHandler(IRealtimePublisher realtimePublisher, IGameRepository gameRepository, ILogger<DiceRolledDomainEventHandler> logger)
    : IDomainEventHandler<DiceRolledDomainEvent>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly ILogger<DiceRolledDomainEventHandler> _logger = logger;

    public async Task<Result<DiceRolledDomainEvent>> HandleAsync(DiceRolledDomainEvent domainEvent, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(domainEvent.Id, ct);
        if (game is null) return Result.Failure<DiceRolledDomainEvent>(DomainError.NotFound);

        IReadOnlyList<string> recipients = [.. game.Players.Select(p => p.Id.ToString())];

        var message = new DiceRolledMessage(
            domainEvent.Id.Value,
            domainEvent.Dice1,
            domainEvent.Dice2,
            domainEvent.Dice1 + domainEvent.Dice2,
            game.CurrentPlayerId().Value,
            game.GamePhase.ToString());

        await _realtimePublisher.ToGameUsersAsync(domainEvent.Id, recipients, nameof(DiceRolledDomainEvent), message, ct);

        _logger.LogInformation("Game {GameId}: dice rolled {Dice1}+{Dice2}={Total} by {PlayerId}",
            domainEvent.Id.Value, domainEvent.Dice1, domainEvent.Dice2, domainEvent.Dice1 + domainEvent.Dice2, game.CurrentPlayerId().Value);

        return Result.Success(domainEvent);
    }
}

public record DiceRolledMessage(Guid GameId, int Dice1, int Dice2, int Total, string CurrentPlayerId, string GamePhase);
