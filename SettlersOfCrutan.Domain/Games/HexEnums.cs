namespace SettlersOfCrutan.Domain.Games;

// Vertex corners around a hex, clockwise starting at top-right for pointy-top orientation
public enum VertexCorner
{
    TopRight = 0,
    Right = 1,
    BottomRight = 2,
    BottomLeft = 3,
    Left = 4,
    TopLeft = 5
}

// Edge directions for hex sides, clockwise starting at top-right
public enum EdgeDirection
{
    NE = 0,
    E = 1,
    SE = 2,
    SW = 3,
    W = 4,
    NW = 5
}
