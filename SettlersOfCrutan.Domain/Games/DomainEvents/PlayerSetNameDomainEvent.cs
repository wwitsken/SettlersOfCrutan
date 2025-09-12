using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record PlayerSetNameDomainEvent(GameId GameId, PlayerId PlayerId, string Name) : IDomainEvent;
