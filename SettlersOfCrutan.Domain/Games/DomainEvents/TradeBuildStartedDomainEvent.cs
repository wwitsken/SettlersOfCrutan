using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record TradeBuildStartedDomainEvent(GameId GameId, PlayerId PlayerId) : IDomainEvent;
