using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.PlayYearOfPlenty;

public record PlayYearOfPlentyContext(
    GamePhase GamePhase,
    PlayerId CurrentPlayerId,
    PlayerId ActingPlayerId,
    Player ActingPlayer,
    ResourceCardType Type1,
    ResourceCardType Type2,
    ResourceHand BankResourceHand);
