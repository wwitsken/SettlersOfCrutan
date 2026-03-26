using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.DomainEvents;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Application.Games.DomainEventHandlers;

public sealed class RobberResolvedDomainEventHandler(IRealtimePublisher realtimePublisher, IGameRepository gameRepository, ILogger<RobberResolvedDomainEventHandler> logger)
    : IDomainEventHandler<RobberResolvedDomainEvent>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly ILogger<RobberResolvedDomainEventHandler> _logger = logger;

    public async Task<Result<RobberResolvedDomainEvent>> HandleAsync(RobberResolvedDomainEvent domainEvent, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(domainEvent.Id, ct);
        if (game is null) return Result.Failure<RobberResolvedDomainEvent>(DomainError.NotFound);

        IReadOnlyList<string> allRecipients = [.. game.Players.Select(p => p.Id.ToString())];
        var newHex = domainEvent.newRobberHexCoord;

        // Public notification: robber moved, who robbed whom (resource type hidden from other players)
        var publicMessage = new RobberResolvedPublicMessage(
            domainEvent.Id.Value,
            domainEvent.RobberId.Value,
            domainEvent.VictimId.Value,
            new HexCoordMessage(newHex.Q, newHex.R, newHex.S),
            domainEvent.StolenResourceType != ResourceCardType.None);

        await _realtimePublisher.ToGameUsersAsync(domainEvent.Id, allRecipients, nameof(RobberResolvedDomainEvent), publicMessage, ct);

        // Private notification to the robber: reveal what resource was stolen
        if (domainEvent.StolenResourceType != ResourceCardType.None)
        {
            var privateMessage = new RobberStolenResourceMessage(
                domainEvent.Id.Value,
                domainEvent.StolenResourceType.ToString());

            await _realtimePublisher.ToGameUserAsync(domainEvent.Id, domainEvent.RobberId.ToString(), nameof(RobberStolenResourceMessage), privateMessage, ct);
        }

        _logger.LogInformation("Game {GameId}: player {RobberId} robbed {VictimId}, stole {Resource}",
            domainEvent.Id.Value, domainEvent.RobberId.Value, domainEvent.VictimId.Value, domainEvent.StolenResourceType);

        return Result.Success(domainEvent);
    }
}

public record HexCoordMessage(int Q, int R, int S);
public record RobberResolvedPublicMessage(Guid GameId, string RobberId, string VictimId, HexCoordMessage NewRobberHex, bool CardWasStolen);
public record RobberStolenResourceMessage(Guid GameId, string StolenResourceType);
