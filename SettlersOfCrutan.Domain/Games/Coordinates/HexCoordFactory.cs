namespace SettlersOfCrutan.Domain.Games.Coordinates;
public static class HexCoordFactory
{
    public static List<HexCoord> GetFromAdjacentEdge(Edge edgeCoord)
    {
        // For a border edge, only one of the two hexes should be present on the board; the other is outside.
        // This method returns the two potential hex coordinates. The test filters by board.Hexes to keep in-bounds.
        // However, to align with test expectation of exactly one hex match, we should return unique normalized edge hexes
        // and let consumers filter against existing hexes; they will end up with one.
        return [edgeCoord.HexCoord1, edgeCoord.HexCoord2];
    }

    public static List<HexCoord> GetFromAdjacentVertex(Vertex vertexCoord)
    {
        return [vertexCoord.HexCoord1, vertexCoord.HexCoord2, vertexCoord.HexCoord3];
    }
}
