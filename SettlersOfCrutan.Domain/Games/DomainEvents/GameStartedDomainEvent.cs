using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record GameStartedDomainEvent(GameId GameId, PlayerId StartingPlayerId) : IDomainEvent;
