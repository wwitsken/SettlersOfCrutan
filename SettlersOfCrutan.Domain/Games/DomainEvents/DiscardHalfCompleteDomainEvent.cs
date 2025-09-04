using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record DiscardHalfCompleteDomainEvent(GameId GameId) : IDomainEvent;