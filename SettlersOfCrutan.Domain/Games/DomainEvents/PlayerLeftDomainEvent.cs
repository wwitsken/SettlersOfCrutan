using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record PlayerLeftDomainEvent(GameId GameId, PlayerId PlayerId, DateTimeOffset When) : IDomainEvent;
