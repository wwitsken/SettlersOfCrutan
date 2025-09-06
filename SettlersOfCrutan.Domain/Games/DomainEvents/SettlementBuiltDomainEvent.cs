using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record SettlementBuiltDomainEvent(GameId GameId, PlayerId PlayerId, PopulationCenter Settlement) : IDomainEvent;
