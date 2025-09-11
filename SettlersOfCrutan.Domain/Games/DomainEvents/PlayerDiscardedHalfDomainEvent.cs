using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record PlayerDiscardedHalfDomainEvent(GameId GameId, PlayerId PlayerId, List<ResourceCardAmount> ResourceAmounts) : IDomainEvent;
