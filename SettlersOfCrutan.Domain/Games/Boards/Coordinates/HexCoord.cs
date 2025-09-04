using System.Globalization;

namespace SettlersOfCrutan.Domain.Games.Boards.Coordinates;

public readonly record struct HexCoord(int Q, int R, int S)
{
    public Dictionary<EdgeDirection, HexCoord> GetAdjacentHexCoords() =>
        new()
        {
            { EdgeDirection.North,     new HexCoord(Q, R - 1, S + 1) },
            { EdgeDirection.NorthEast, new HexCoord(Q + 1, R - 1, S) },
            { EdgeDirection.SouthEast, new HexCoord(Q + 1, R, S - 1) },
            { EdgeDirection.South,     new HexCoord(Q, R + 1, S - 1) },
            { EdgeDirection.SouthWest, new HexCoord(Q - 1, R + 1, S) },
            { EdgeDirection.NorthWest, new HexCoord(Q - 1, R, S + 1) }
        };

    public override string ToString() => $"{Q}:{R}:{S}";
    public string ToIdString() => FormattableString.Invariant($"{Q}:{R}");
    public string ToIdString3() => FormattableString.Invariant($"{Q}:{R}:{S}");

    public static bool TryParseId(string value, out HexCoord coord)
    {
        coord = default;
        if (string.IsNullOrWhiteSpace(value)) return false;

        var parts = value.Split(':');
        if (parts.Length is not (2 or 3)) return false;

        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var q)) return false;
        if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)) return false;

        if (parts.Length == 2)
        {
            var s = -q - r;
            coord = new HexCoord(q, r, s);
            return true;
        }

        if (!int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var s3)) return false;
        if (q + r + s3 != 0) return false; // enforce cube constraint
        coord = new HexCoord(q, r, s3);
        return true;
    }

    public static HexCoord ParseId(string value)
        => TryParseId(value, out var c) ? c : throw new FormatException($"Invalid HexCoord ID: '{value}'");

    public List<Vertex> GetAdjacentVertices() =>
        [.. VertexFactory.FromHex(this)];
}
