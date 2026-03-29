using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.ProposeTrade;

public record ProposeTradeContext(
    GamePhase GamePhase,
    PlayerId CurrentPlayerId,
    PlayerId ActingPlayerId,
    TradeOffer? CurrentTradeOffer,
    Player? Proposer,
    List<ResourceCardAmount> Offered
);
