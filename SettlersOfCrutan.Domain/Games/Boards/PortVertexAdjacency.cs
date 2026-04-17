namespace SettlersOfCrutan.Domain.Games.Boards;

/// <summary>
/// Maritime ports sit on a board edge (two adjacent hexes). A settlement/city may use that port
/// only when built on the vertex incident to that edge — i.e. the vertex’s three hex coordinates
/// must include <b>both</b> hexes of the port edge (sharing a single hex is not enough).
/// </summary>
public static class PortVertexAdjacency
{
    public static bool PopulationCenterTouchesPort(Port port, PopulationCenter pc)
    {
        var edgeHexes = port.EdgeCoordinate.Normalize().HexCoords();
        var vertexHexes = pc.VertexCoordinate.Normalize().HexCoords();
        return edgeHexes.TrueForAll(h => vertexHexes.Contains(h));
    }
}
