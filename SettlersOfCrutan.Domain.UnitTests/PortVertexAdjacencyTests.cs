using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Domain.UnitTests;

public class PortVertexAdjacencyTests
{
    private static readonly HexCoord Origin = new(0, 0, 0);

    /// <summary>
    /// Port on edge Origin–NE neighbor; settlement only shares Origin with that edge (wrong vertex) must not count.
    /// </summary>
    [Fact]
    public void PopulationCenterTouchesPort_FalseWhenVertexSharesOnlyOneEdgeHex()
    {
        var ne = new HexCoord(1, -1, 0);
        var port = new Port(new Edge(Origin, ne)) { Type = PortType.Brick2to1 };
        var wrongVertex = VertexFactory.FromHexCorner(Origin, HexCornerDirection.NW).Normalize();
        var pc = PopulationCenter.CreateSettlement(wrongVertex, new PlayerId { Value = "p1" });

        Assert.False(PortVertexAdjacency.PopulationCenterTouchesPort(port, pc));
    }

    [Fact]
    public void PopulationCenterTouchesPort_TrueWhenVertexIncludesBothEdgeHexes()
    {
        var ne = new HexCoord(1, -1, 0);
        var port = new Port(new Edge(Origin, ne)) { Type = PortType.Brick2to1 };
        var onPortVertex = VertexFactory.FromHexCorner(Origin, HexCornerDirection.NE).Normalize();
        var pc = PopulationCenter.CreateSettlement(onPortVertex, new PlayerId { Value = "p1" });

        Assert.True(PortVertexAdjacency.PopulationCenterTouchesPort(port, pc));
    }
}
