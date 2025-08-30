namespace SettlersOfCrutan.Domain.Games.Coordinates;
public static class VertexFactory
{
    private static readonly Dictionary<HexEdgeDirection, (int dX, int dY, int dZ)> EdgeOffsets = new()
    {
        [HexEdgeDirection.N] = (0, -1, 1),
        [HexEdgeDirection.NE] = (1, -1, 0),
        [HexEdgeDirection.SE] = (1, 0, -1),
        [HexEdgeDirection.S] = (0, 1, -1),
        [HexEdgeDirection.SW] = (-1, 1, 0),
        [HexEdgeDirection.NW] = (-1, 0, 1),
    };


    public static Vertex FromHexCorner(HexCoord hex, HexCornerDirection corner)
    {
        // Plan:
        // 1. Each corner of a hex is shared by three hexes.
        // 2. For a given hex and corner, find the two adjacent hexes that share that corner.
        // 3. Return a VertexCoord with the three hexes.

        // Hex corners in axial coordinates:
        // NE: hex, hex + NE, hex + NW
        // E:  hex, hex + SE, hex + NE
        // SE: hex, hex + S,  hex + SE
        // SW: hex, hex + SW, hex + S
        // W:  hex, hex + NW, hex + SW
        // NW: hex, hex + N,  hex + NW

        HexCoord h2, h3;
        switch (corner)
        {
            case HexCornerDirection.NE:
                h2 = FromHexEdge(hex, HexEdgeDirection.NE);
                h3 = FromHexEdge(hex, HexEdgeDirection.NW);
                break;
            case HexCornerDirection.E:
                h2 = FromHexEdge(hex, HexEdgeDirection.SE);
                h3 = FromHexEdge(hex, HexEdgeDirection.NE);
                break;
            case HexCornerDirection.SE:
                h2 = FromHexEdge(hex, HexEdgeDirection.S);
                h3 = FromHexEdge(hex, HexEdgeDirection.SE);
                break;
            case HexCornerDirection.SW:
                h2 = FromHexEdge(hex, HexEdgeDirection.SW);
                h3 = FromHexEdge(hex, HexEdgeDirection.S);
                break;
            case HexCornerDirection.W:
                h2 = FromHexEdge(hex, HexEdgeDirection.NW);
                h3 = FromHexEdge(hex, HexEdgeDirection.SW);
                break;
            case HexCornerDirection.NW:
                h2 = FromHexEdge(hex, HexEdgeDirection.N);
                h3 = FromHexEdge(hex, HexEdgeDirection.NW);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(corner), corner, null);
        }

        return new Vertex(hex, h2, h3).Normalize();
    }

    public static HexCoord FromHexEdge(HexCoord hex, HexEdgeDirection edgeDirection)
    {
        var (dQ, dR, dS) = EdgeOffsets[edgeDirection];
        return new HexCoord(hex.Q + dQ, hex.R + dR, hex.S + dS);
    }

    public static Vertex[] FromHex(HexCoord hex)
    {
        var corners = new[]
        {
            HexCornerDirection.NE,
            HexCornerDirection.E,
            HexCornerDirection.SE,
            HexCornerDirection.SW,
            HexCornerDirection.W,
            HexCornerDirection.NW
        };

        var vertices = new Vertex[corners.Length];
        for (int i = 0; i < corners.Length; i++)
        {
            vertices[i] = FromHexCorner(hex, corners[i]);
        }
        return vertices;
    }

    public static Vertex[] GetAdjacentVertices(Vertex v)
    {
        var vertices = new HashSet<Vertex>();

        var hexes = new[] { v.HexCoord1, v.HexCoord2, v.HexCoord3 };

        // For each pair of hexes, find the third hex adjacent to both
        for (int i = 0; i < 3; i++)
        {
            var h1 = hexes[i];
            var h2 = hexes[(i + 1) % 3];

            // Get adjacent hexes to h1
            var adjH1 = h1.GetAdjacentHexCoords().Values;
            // Get adjacent hexes to h2
            var adjH2 = h2.GetAdjacentHexCoords().Values;

            // Find common adjacent hex
            foreach (var candidate in adjH1)
            {
                if (adjH2.Contains(candidate) && candidate != hexes[(i + 2) % 3])
                {
                    // Build new vertex with the two shared hexes and the new adjacent hex
                    var newVertex = new Vertex(h1, h2, candidate).Normalize();
                    if (!newVertex.Equals(v.Normalize()))
                    {
                        vertices.Add(newVertex);
                    }
                }
            }
        }

        return [.. vertices];
    }
}
