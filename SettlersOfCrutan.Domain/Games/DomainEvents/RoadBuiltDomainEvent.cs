using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record RoadBuiltDomainEvent(GameId GameId, PlayerId PlayerId, Road Road) : IDomainEvent;
