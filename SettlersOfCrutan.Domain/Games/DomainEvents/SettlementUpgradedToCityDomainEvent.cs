using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record SettlementUpgradedToCityDomainEvent(GameId GameId, PlayerId PlayerId, PopulationCenter City) : IDomainEvent;
