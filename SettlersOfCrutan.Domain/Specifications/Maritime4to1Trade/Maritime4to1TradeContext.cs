using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.Maritime4to1Trade;

public record Maritime4to1TradeContext(
    GamePhase GamePhase,
    PlayerId CurrentPlayerId,
    PlayerId ActingPlayerId,
    Player ActingPlayer,
    ResourceCardType DiscardResource,
    ResourceCardType RequestResource,
    ResourceHand BankResourceHand
);
