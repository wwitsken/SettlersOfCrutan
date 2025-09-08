using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record MaritimeTradeExecutedDomainEvent(GameId GameId, PlayerId PlayerId, List<ResourceAmount> PlayerTrade) : IDomainEvent;
