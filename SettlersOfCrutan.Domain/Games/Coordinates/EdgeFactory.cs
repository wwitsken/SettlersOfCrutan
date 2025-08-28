namespace SettlersOfCrutan.Domain.Games.Coordinates;
public static class EdgeFactory
{
    public static bool ConnectsToEdge(Edge edge, Edge other)
    {
        List<HexCoord> edgeHexes = [edge.HexCoord1, edge.HexCoord2];
        List<HexCoord> otherHexes = [other.HexCoord1, other.HexCoord2];
        return edgeHexes.Intersect(otherHexes).Any();
    }
}
