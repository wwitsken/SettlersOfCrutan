using Microsoft.Extensions.Logging;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.DomainEvents;

namespace SettlersOfCrutan.Application.Games.DomainEventHandlers;
public sealed class InitialSettlementAndRoadPlacedDomainEventHandler(IRealtimePublisher realtimePublisher, ILogger<InitialSettlementAndRoadPlacedDomainEventHandler> logger)
    : IDomainEventHandler<InitialSettlementAndRoadPlacedDomainEvent>
{
    private readonly IRealtimePublisher _realtimePublisher = realtimePublisher;
    private readonly ILogger<InitialSettlementAndRoadPlacedDomainEventHandler> _logger = logger;

    public async Task<Result<InitialSettlementAndRoadPlacedDomainEvent>> HandleAsync(InitialSettlementAndRoadPlacedDomainEvent domainEvent, CancellationToken ct = default)
    {
        var message = new InitialSettlementAndRoadPlacedMessage(
            domainEvent.GameId.Value,
            domainEvent.PlayerId.Value,
            domainEvent.Settlement.VertexCoordinate,
            domainEvent.Road.EdgeCoordinate);

        await _realtimePublisher.PublishToGroupAsync(domainEvent.GameId.Value.ToString(), message, ct);

        _logger.LogInformation("Player has placed initial road and settlmenet successfully");

        return Result<InitialSettlementAndRoadPlacedDomainEvent>.Success(domainEvent);
    }
}

public record InitialSettlementAndRoadPlacedMessage(Guid GameId, string PlayerId, Vertex SettlementCoordinate, Edge RoadCoordinate);
