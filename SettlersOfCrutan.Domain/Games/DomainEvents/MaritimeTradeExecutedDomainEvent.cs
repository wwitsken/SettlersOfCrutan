using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record MaritimeTradeExecutedDomainEvent(GameId GameId, PlayerId PlayerId, List<ResourceAmount<ResourceCardType>> PlayerTrade) : IDomainEvent;
