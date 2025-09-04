using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record DiscardHalfStartedDomainEvent(GameId GameId, List<PlayerId> RequiredPlayersToDiscard) : IDomainEvent;
