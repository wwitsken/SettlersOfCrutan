using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.Specifications.RollAndResolveProduction;

public record RollAndResolveProductionContext(
    GamePhase GamePhase,
    PlayerId CurrentPlayerId,
    PlayerId ActingPlayerId
);
