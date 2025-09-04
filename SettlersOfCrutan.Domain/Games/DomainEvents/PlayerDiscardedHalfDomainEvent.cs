using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record PlayerDiscardedHalfDomainEvent(GameId GameId, PlayerId PlayerId, List<ResourceAmount> ResourceAmounts) : IDomainEvent;
