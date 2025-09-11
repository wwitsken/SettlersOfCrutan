using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record TradeOfferPostedDomainEvent(
    GameId GameId,
    TradeOfferId TradeOfferId,
    PlayerId ProposerId,
    List<ResourceCardAmount> Requested,
    List<ResourceCardAmount> Offered
) : IDomainEvent;
