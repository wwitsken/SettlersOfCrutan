using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;

namespace SettlersOfCrutan.Domain.Specifications.EndTurn;

public record EndTurnContext(
    GamePhase GamePhase,
    int GameRound,
    PlayerDirection PlayerDirection,
    List<Road> PlayerRoads,
    List<PopulationCenter> PlayerPopulationCenters,
    PlayerId CurrentPlayerId,
    PlayerId ActingPlayerId
);
