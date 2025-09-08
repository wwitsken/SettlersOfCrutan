using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record PlayerJoinedDomainEvent(GameId GameId, PlayerId PlayerId, DateTimeOffset When) : IDomainEvent;
