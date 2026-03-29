using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Domain.Specifications.PlaceInitial;

public record PlaceInitialContext(
    GamePhase GamePhase,
    PlayerId CurrentPlayerId,
    PlayerId ActingPlayerId,
    Player ActingPlayer,
    Board Board,
    Vertex SettlementVertex,
    Edge RoadEdge
);
