namespace SettlersOfCrutan.Domain.Games;

// Pointy-top axial coordinates (q, r), with s = -q - r
public readonly record struct AxialCoord(int Q, int R)
{
    public int S => -Q - R;
    public override string ToString() => $"({Q},{R},{S})";
}
