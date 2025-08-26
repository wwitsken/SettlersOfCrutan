namespace SettlersOfCrutan.Domain.Games;

public readonly record struct VertexCoord(AxialCoord Hex, VertexCorner Corner)
{
    public override string ToString() => $"{Hex}:{Corner}";
}

public readonly record struct EdgeCoord(AxialCoord Hex, EdgeDirection Direction)
{
    public override string ToString() => $"{Hex}:{Direction}";
}
