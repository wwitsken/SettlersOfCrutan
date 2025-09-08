using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record KnightCardPlayedDomainEvent(GameId GameId, PlayerId PlayerId) : IDomainEvent;
