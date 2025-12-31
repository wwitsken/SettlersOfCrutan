using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.DomainEvents;

namespace SettlersOfCrutan.Application.Games.DomainEventHandlers;
public sealed class InitialSettlementAndRoadPlacedDomainEventHandler(IRealtimePublisher realtimePublisher, IGameRepository gameRepository, ILogger<InitialSettlementAndRoadPlacedDomainEventHandler> logger)
    : IDomainEventHandler<InitialSettlementAndRoadPlacedDomainEvent>
{
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly ILogger<InitialSettlementAndRoadPlacedDomainEventHandler> _logger = logger;

    public async Task<Result<InitialSettlementAndRoadPlacedDomainEvent>> HandleAsync(InitialSettlementAndRoadPlacedDomainEvent domainEvent, CancellationToken ct = default)
    {
        var game = await _gameRepository.GetAsync(domainEvent.GameId, ct);
        if (game is null) return Result.Failure<InitialSettlementAndRoadPlacedDomainEvent>(DomainError.NotFound);
        IReadOnlyList<string> recipients = [.. game.Players
            .Select(p => p.Id.ToString())];

        var message = new InitialSettlementAndRoadPlacedMessage(
            domainEvent.GameId.Value,
            domainEvent.PlayerId.Value,
            domainEvent.Settlement.VertexCoordinate,
            domainEvent.Road.EdgeCoordinate);

        await _realtimePublisher.UpdateGameAsync(domainEvent.GameId, recipients, DateTimeOffset.Now, nameof(InitialSettlementAndRoadPlacedDomainEvent), message, ct);

        _logger.LogInformation("Player has placed initial road and settlmenet successfully");

        return Result<InitialSettlementAndRoadPlacedDomainEvent>.Success(domainEvent);
    }
}

public record InitialSettlementAndRoadPlacedMessage(Guid GameId, string PlayerId, Vertex SettlementCoordinate, Edge RoadCoordinate);
public record HexCoordDto(int Q, int R, int S);
public record VertexDto(HexCoordDto HexCoord1, HexCoordDto HexCoord2, HexCoordDto HexCoord3);
public record EdgeDto(HexCoordDto HexCoord1, HexCoordDto HexCoord2);