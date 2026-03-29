using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.Maritime3to1Trade;

public record Maritime3to1TradeContext(
    GamePhase GamePhase,
    PlayerId CurrentPlayerId,
    PlayerId ActingPlayerId,
    Player ActingPlayer,
    ResourceCardType DiscardResource,
    ResourceCardType RequestResource,
    ResourceHand BankResourceHand,
    Board Board
);
