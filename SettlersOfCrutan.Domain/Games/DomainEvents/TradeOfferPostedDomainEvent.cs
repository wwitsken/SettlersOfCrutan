using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record TradeOfferPostedDomainEvent(
    GameId GameId,
    TradeOfferId TradeOfferId,
    PlayerId ProposerId,
    List<ResourceAmount> Requested,
    List<ResourceAmount> Offered
) : IDomainEvent;
