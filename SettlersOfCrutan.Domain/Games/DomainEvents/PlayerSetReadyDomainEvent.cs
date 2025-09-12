using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record PlayerSetReadyDomainEvent(GameId GameId, PlayerId PlayerId, bool Ready) : IDomainEvent;
