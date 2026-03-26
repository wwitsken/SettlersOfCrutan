using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.DomainEvents;

namespace SettlersOfCrutan.Application.Games.DomainEventHandlers;

public sealed class RobPlayerStartedDomainEventHandler(IRealtimePublisher realtimePublisher, IGameRepository gameRepository, ILogger<RobPlayerStartedDomainEventHandler> logger)
    : IDomainEventHandler<RobPlayerStartedDomainEvent>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly ILogger<RobPlayerStartedDomainEventHandler> _logger = logger;

    public async Task<Result<RobPlayerStartedDomainEvent>> HandleAsync(RobPlayerStartedDomainEvent domainEvent, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(domainEvent.GameId, ct);
        if (game is null) return Result.Failure<RobPlayerStartedDomainEvent>(DomainError.NotFound);

        IReadOnlyList<string> allRecipients = [.. game.Players.Select(p => p.Id.ToString())];
        var activePlayerId = game.CurrentPlayerId();

        // Notify all players who must move the robber
        var publicMessage = new RobPlayerStartedMessage(domainEvent.GameId.Value, activePlayerId.Value);
        await _realtimePublisher.ToGameUsersAsync(domainEvent.GameId, allRecipients, nameof(RobPlayerStartedDomainEvent), publicMessage, ct);

        _logger.LogInformation("Game {GameId}: player {PlayerId} must move the robber and choose a victim",
            domainEvent.GameId.Value, activePlayerId.Value);

        return Result.Success(domainEvent);
    }
}

public record RobPlayerStartedMessage(Guid GameId, string ActivePlayerId);
