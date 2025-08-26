namespace SettlersOfCrutan.Domain.Games;

public static class HexTopology
{
    // Neighbor deltas for pointy-top axial coords (q,r)
    private static readonly (int dq, int dr)[] NeighborDirs =
    [
        (1, -1), // NE
        (1, 0),  // E
        (0, 1),  // SE
        (-1, 1), // SW
        (-1, 0), // W
        (0, -1), // NW
    ];

    public static AxialCoord Neighbor(AxialCoord a, EdgeDirection dir)
    {
        var (dq, dr) = NeighborDirs[(int)dir];
        return new AxialCoord(a.Q + dq, a.R + dr);
    }

    public static EdgeDirection Opposite(EdgeDirection d) => (EdgeDirection)(((int)d + 3) % 6);

    private static int Compare(AxialCoord a, AxialCoord b)
    {
        var c = a.Q.CompareTo(b.Q);
        if (c != 0) return c;
        return a.R.CompareTo(b.R);
    }

    // Canonicalize an edge coordinate to a unique representation shared by both adjacent hexes
    public static EdgeCoord Canonicalize(EdgeCoord edge)
    {
        var alt = new EdgeCoord(Neighbor(edge.Hex, edge.Direction), Opposite(edge.Direction));
        // choose lexicographically smallest (Q,R,Direction)
        var cmp = Compare(edge.Hex, alt.Hex);
        if (cmp < 0) return edge;
        if (cmp > 0) return alt;
        return (int)edge.Direction <= (int)alt.Direction ? edge : alt;
    }

    // Given a hex edge, return the two vertex coords at its ends (canonicalizing to this hex's corner indices)
    public static (VertexCoord a, VertexCoord b) GetEdgeVertices(EdgeCoord edge)
    {
        // Map edge direction to the two corners it connects on the reference hex
        var corners = edge.Direction switch
        {
            EdgeDirection.NE => (VertexCorner.TopRight, VertexCorner.Right),
            EdgeDirection.E  => (VertexCorner.Right, VertexCorner.BottomRight),
            EdgeDirection.SE => (VertexCorner.BottomRight, VertexCorner.BottomLeft),
            EdgeDirection.SW => (VertexCorner.BottomLeft, VertexCorner.Left),
            EdgeDirection.W  => (VertexCorner.Left, VertexCorner.TopLeft),
            EdgeDirection.NW => (VertexCorner.TopLeft, VertexCorner.TopRight),
            _ => throw new ArgumentOutOfRangeException()
        };
        var a = new VertexCoord(edge.Hex, corners.Item1);
        var b = new VertexCoord(edge.Hex, corners.Item2);
        return (a, b);
    }

    // Canonicalize a vertex coordinate to a unique representation shared by up to three adjacent hexes
    public static VertexCoord Canonicalize(VertexCoord vertex)
    {
        var eq = GetEquivalentVertexCoords(vertex);
        // pick lexicographically smallest by (Q,R,cornerIndex)
        VertexCoord best = vertex;
        foreach (var v in eq)
        {
            var cmp = Compare(v.Hex, best.Hex);
            if (cmp < 0 || (cmp == 0 && (int)v.Corner < (int)best.Corner))
            {
                best = v;
            }
        }
        return best;
    }

    private static IEnumerable<VertexCoord> GetEquivalentVertexCoords(VertexCoord vertex)
    {
        var h = vertex.Hex; var i = (int)vertex.Corner;
        // Two neighboring hexes that share this vertex are along directions i and (i-1)
        var dir1 = (EdgeDirection)i;
        var dir2 = (EdgeDirection)((i + 5) % 6);
        // Mapping for neighbor corner indices in pointy-top axial:
        // across dir1, corner becomes (i+4)%6; across dir2, corner becomes (i+2)%6
        var v0 = vertex;
        var v1 = new VertexCoord(Neighbor(h, dir1), (VertexCorner)((i + 4) % 6));
        var v2 = new VertexCoord(Neighbor(h, dir2), (VertexCorner)((i + 2) % 6));
        return new [] { v0, v1, v2 };
    }

    // For a given vertex, return the three incident edges (canonicalized)
    public static IEnumerable<EdgeCoord> GetVertexEdges(VertexCoord vertex)
    {
        var reps = GetEquivalentVertexCoords(Canonicalize(vertex));
        var set = new HashSet<EdgeCoord>();
        foreach (var v in reps)
        {
            var i = (int)v.Corner;
            var e1 = Canonicalize(new EdgeCoord(v.Hex, (EdgeDirection)i));
            var e2 = Canonicalize(new EdgeCoord(v.Hex, (EdgeDirection)((i + 5) % 6)));
            set.Add(e1);
            set.Add(e2);
        }
        return set; // should be 3 unique edges
    }

    // Adjacent edges share a vertex with the given edge (canonicalized)
    public static IEnumerable<EdgeCoord> GetAdjacentEdges(EdgeCoord edge)
    {
        var ce = Canonicalize(edge);
        var (va, vb) = GetEdgeVertices(ce);
        var edges = GetVertexEdges(va).Concat(GetVertexEdges(vb))
            .Select(Canonicalize)
            .Where(e => !e.Equals(ce))
            .Distinct();
        return edges;
    }

    // For a given vertex, return the three adjacent vertices (canonicalized)
    public static IEnumerable<VertexCoord> GetAdjacentVertices(VertexCoord vertex)
    {
        var cv = Canonicalize(vertex);
        var neighbors = new HashSet<VertexCoord>();
        foreach (var e in GetVertexEdges(cv))
        {
            var (a, b) = GetEdgeVertices(e);
            var ca = Canonicalize(a);
            var cb = Canonicalize(b);
            var other = ca.Equals(cv) ? cb : ca;
            neighbors.Add(other);
        }
        return neighbors;
    }
}
