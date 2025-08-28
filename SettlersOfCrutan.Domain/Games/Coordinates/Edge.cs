namespace SettlersOfCrutan.Domain.Games.Coordinates;
public readonly record struct Edge(HexCoord HexCoord1, HexCoord HexCoord2)
{
    // Normalize ordering so (a,b) == (b,a)
    public Edge Normalize()
    {
        // Compare tuple values to impose a total order
        return Compare(HexCoord1, HexCoord2) <= 0 ? this : new Edge(HexCoord2, HexCoord1);

        static int Compare(HexCoord x, HexCoord y)
        {
            // lexicographic compare (A,B,C)
            if (x.Q != y.Q) return x.Q.CompareTo(y.Q);
            if (x.R != y.R) return x.R.CompareTo(y.R);
            return x.S.CompareTo(y.S);
        }
    }

    public override string ToString()
    {
        var norm = Normalize();
        return $"{norm.HexCoord1.ToIdString()}::{norm.HexCoord2.ToIdString()}";
    }
}