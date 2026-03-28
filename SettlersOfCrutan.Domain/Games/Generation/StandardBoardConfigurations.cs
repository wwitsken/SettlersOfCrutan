using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games.Generation;

public static class StandardBoardConfigurations
{
    /// <summary>
    /// Standard base-game (radius 2) port edge positions: land hex then adjacent off-board hex.
    /// Port <i>types</i> are still assigned by the board generator (random or deterministic).
    /// </summary>
    public static IReadOnlyList<PortEdgeSlot> DefaultBaseGamePortEdges { get; } =
    [
        new(new HexCoord(1, -2, 1), new HexCoord(2, -3, 1)),
        new(new HexCoord(2, -1, -1), new HexCoord(3, -2, -1)),
        new(new HexCoord(2, 0, -2), new HexCoord(3, 0, -3)),
        new(new HexCoord(1, 1, -2), new HexCoord(1, 2, -3)),
        new(new HexCoord(-1, 2, -1), new HexCoord(-1, 3, -2)),
        new(new HexCoord(-2, 2, 0), new HexCoord(-3, 3, 0)),
        new(new HexCoord(-2, 1, 1), new HexCoord(-3, 1, 2)),
        new(new HexCoord(-1, -1, 2), new HexCoord(-2, -1, 3)),
        new(new HexCoord(0, -2, 2), new HexCoord(0, -3, 3)),
    ];

    public static BoardConfig DefaultBaseGame => new
        (
        Radius: 2,
        ResourceCounts: new Dictionary<ResourceCardType, int>
        {
            [ResourceCardType.Brick] = 3,
            [ResourceCardType.Lumber] = 4,
            [ResourceCardType.Wool] = 4,
            [ResourceCardType.Grain] = 4,
            [ResourceCardType.Ore] = 3,
            [ResourceCardType.Desert] = 1,
        },
        NumberTokens: [2, 3, 3, 4, 4, 5, 5, 6, 6, 8, 8, 9, 9, 10, 10, 11, 11, 12],
        Ports: new Dictionary<PortType, int>
        {
            [PortType.Generic3to1] = 4,
            [PortType.Brick2to1] = 1,
            [PortType.Lumber2to1] = 1,
            [PortType.Wool2to1] = 1,
            [PortType.Grain2to1] = 1,
            [PortType.Ore2to1] = 1
        },
        FixedPortEdges: DefaultBaseGamePortEdges);
}
