using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record InitialSettlementAndRoadPlaceDomainEvent(GameId GameId, PlayerId PlayerId, PopulationCenter Settlement, Road Road) : IDomainEvent;