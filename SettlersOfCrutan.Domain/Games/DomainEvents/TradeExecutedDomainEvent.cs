using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record TradeExecutedDomainEvent(
    GameId GameId,
    TradeOfferId TradeOfferId,
    PlayerId ProposerId,
    PlayerId AcceptorId,
    List<ResourceCardAmount> Offered,
    List<ResourceCardAmount> Requested
) : IDomainEvent;
