using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.BuyDevelopmentCard;

public record BuyDevelopmentCardContext(
    GamePhase GamePhase,
    PlayerId CurrentPlayerId,
    PlayerId ActingPlayerId,
    Player ActingPlayer,
    List<ResourceCardAmount> Cost,
    int PlayerDevCardCount,
    int BankDevCardCount
);
