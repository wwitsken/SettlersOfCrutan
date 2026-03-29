namespace SettlersOfCrutan.Domain.Games.Boards.Coordinates;

public static class EdgeFactory
{
    /// <summary>
    /// True when the two edges share at least one endpoint vertex (roads may extend from a common corner).
    /// Edges on the same hex but opposite sides share only that hex, not a vertex — they must not count as connected.
    /// </summary>
    public static bool ConnectsToEdge(Edge edge, Edge other)
    {
        var e1 = edge.Normalize();
        var e2 = other.Normalize();
        if (e1.Equals(e2)) return false;

        if (!VertexFactory.TryEndpointsForEdge(e1, out var a1, out var a2)) return false;
        if (!VertexFactory.TryEndpointsForEdge(e2, out var b1, out var b2)) return false;

        var na1 = a1.Normalize();
        var na2 = a2.Normalize();
        var nb1 = b1.Normalize();
        var nb2 = b2.Normalize();
        return na1.Equals(nb1) || na1.Equals(nb2) || na2.Equals(nb1) || na2.Equals(nb2);
    }

    public static Edge FromHexEdge(HexCoord hex, EdgeDirection edgeDirection)
    {
        HexCoord otherHex = hex.GetAdjacentHexCoords()[edgeDirection];
        return new Edge(hex, otherHex).Normalize();
    }

}
