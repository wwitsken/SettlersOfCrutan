using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record InitialSettlementAndRoadPlacedDomainEvent(GameId GameId, PlayerId PlayerId, PopulationCenter Settlement, Road Road) : IDomainEvent;