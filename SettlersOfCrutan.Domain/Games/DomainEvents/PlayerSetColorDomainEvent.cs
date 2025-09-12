using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record PlayerSetColorDomainEvent(GameId GameId, PlayerId PlayerId, PlayerColor Color) : IDomainEvent;
