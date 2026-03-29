using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Domain.Specifications.ResolveRobber;

public record ResolveRobberContext(
    GamePhase GamePhase,
    PlayerId CurrentPlayerId,
    PlayerId ActingPlayerId,
    Board Board,
    HexCoord NewRobberHexCoord,
    PlayerId? VictimId,
    IReadOnlyList<PlayerId> StealTargets
);
