using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Specifications.BuildRoad;

public record BuildRoadContext(
    GamePhase GamePhase,
    PlayerId CurrentPlayerId,
    PlayerId ActingPlayerId,
    Player ActingPlayer,
    List<ResourceCardAmount> Cost,
    Board Board,
    Edge Edge
);
