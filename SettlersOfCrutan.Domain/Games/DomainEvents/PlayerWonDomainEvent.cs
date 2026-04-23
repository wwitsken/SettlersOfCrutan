using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;

public record PlayerWonDomainEvent(GameId GameId, PlayerId WinnerPlayerId) : IDomainEvent;
