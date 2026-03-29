using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.EndTurn;

public record EndTurnContext(
    GamePhase GamePhase,
    PlayerId CurrentPlayerId,
    PlayerId ActingPlayerId
);
