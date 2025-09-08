using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record RoadBuildingCardPlayedDomainEvent(GameId GameId, PlayerId PlayerId, Road Road1, Road Road2) : IDomainEvent;
