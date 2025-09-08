using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record TradeExecutedDomainEvent(
    GameId GameId,
    TradeOfferId TradeOfferId,
    PlayerId ProposerId,
    PlayerId AcceptorId,
    List<ResourceAmount> Offered,
    List<ResourceAmount> Requested
) : IDomainEvent;
