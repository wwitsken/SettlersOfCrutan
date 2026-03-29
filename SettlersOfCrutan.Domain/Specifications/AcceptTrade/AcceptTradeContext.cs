using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.AcceptTrade;

public record AcceptTradeContext(
    GamePhase GamePhase,
    TradeOffer? CurrentTradeOffer,
    TradeOfferId TradeOfferId,
    Player? Proposer,
    Player? Acceptor
);
