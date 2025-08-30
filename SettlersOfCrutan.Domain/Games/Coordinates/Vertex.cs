namespace SettlersOfCrutan.Domain.Games.Coordinates;
public readonly record struct Vertex(HexCoord HexCoord1, HexCoord HexCoord2, HexCoord HexCoord3)
{
    public Vertex Normalize()
    {
        // Sort the three HexCoords to impose a total order
        var coords = new[] { HexCoord1, HexCoord2, HexCoord3 };
        Array.Sort(coords, Compare);

        return new Vertex(coords[0], coords[1], coords[2]);

        static int Compare(HexCoord x, HexCoord y)
        {
            if (x.Q != y.Q) return x.Q.CompareTo(y.Q);
            if (x.R != y.R) return x.R.CompareTo(y.R);
            return x.S.CompareTo(y.S);
        }
    }

    public override string ToString()
    {
        var norm = Normalize();
        return $"{norm.HexCoord1.ToIdString()}::{norm.HexCoord2.ToIdString()}::{norm.HexCoord3.ToIdString()}";
    }

    public bool Equals(Vertex other)
    {
        var norm = this.Normalize();
        var otherNorm = other.Normalize();
        return norm.HexCoord1 == otherNorm.HexCoord1 &&
               norm.HexCoord2 == otherNorm.HexCoord2 &&
               norm.HexCoord3 == otherNorm.HexCoord3;
    }

    public override int GetHashCode()
    {
        var norm = Normalize();
        return HashCode.Combine(norm.HexCoord1, norm.HexCoord2, norm.HexCoord3);
    }

    public List<HexCoord> ToList() => [HexCoord1, HexCoord2, HexCoord3];
}