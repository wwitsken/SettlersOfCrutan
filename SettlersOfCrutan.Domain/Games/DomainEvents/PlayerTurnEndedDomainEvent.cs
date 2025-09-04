using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record PlayerTurnEndedDomainEvent(GameId GameId, PlayerId OldPlayer, PlayerId CurrentPlayer) : IDomainEvent;
