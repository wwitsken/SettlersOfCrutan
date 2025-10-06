using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Generation;

/// <summary>
/// Generates a deterministic, standard Catan board layout for the base game (radius = 2).
/// - Desert in the center with the robber
/// - Resources placed in a fixed ring order (not randomized)
/// - Number tokens placed using the classic chit sequence around the rings (outer then inner), skipping the desert
/// - Ports placed deterministically around the border edges in a fixed order
/// </summary>
public class StandardBoardGenerator : IBoardGenerator
{
    public Board Generate(BoardConfig config, int seed)
    {
        // Only the standard base game (radius 2) is supported here.
        if (config.Radius != 2)
        {
            throw new NotSupportedException("StandardBoardGenerator currently supports only radius = 2 (base game).");
        }

        // Build all coordinates once
        var center = new HexCoord(0, 0, 0);
        var ring1 = GetRing(center, 1); // 6 hexes
        var ring2 = GetRing(center, 2); // 12 hexes

        // Validate config counts match expected base game counts
        ValidateCounts(config);

        // Build a deterministic resource sequence for non-desert tiles (18 total)
        // Sequence is constructed to exactly match standard base game counts:
        // Lumber(4), Wool(4), Grain(4), Brick(3), Ore(3)
        var resourceSequence = BuildDeterministicResourceSequence();

        // Classic chit sequence for the 18 non-desert hexes, in order
        // Applied to ring2 first, then ring1 (skipping center desert)
        var numberSequence = s_standardNumberTokenSequence;

        List<Hex> hexes = new(capacity: 19)
        {
            // Center desert with robber
            new Hex(center)
            {
                Resource = ResourceCardType.Desert,
                HasRobber = true,
                NumberToken = null
            }
        };

        // Place resources and numbers on the outer ring (12)
        int idx = 0;
        for (int i = 0; i < ring2.Count; i++)
        {
            hexes.Add(new Hex(ring2[i])
            {
                Resource = resourceSequence[idx],
                NumberToken = numberSequence[idx],
                HasRobber = false
            });
            idx++;
        }

        // Then inner ring (6)
        for (int i = 0; i < ring1.Count; i++)
        {
            hexes.Add(new Hex(ring1[i])
            {
                Resource = resourceSequence[idx],
                NumberToken = numberSequence[idx],
                HasRobber = false
            });
            idx++;
        }

        // Deterministic ports placement: enumerate border edges in a stable order
        var coords = GetHexCoords(config.Radius).ToList();
        var borderEdges = GetBorderEdges(coords).Distinct().OrderBy(e => e.ToString()).ToList();

        var portTypes = BuildDeterministicPortTypes(config.Ports);
        var ports = new List<Port>();
        for (int i = 0; i < Math.Min(borderEdges.Count, portTypes.Count); i++)
        {
            ports.Add(new Port(borderEdges[i]) { Type = portTypes[i] });
        }

        return Board.Create(hexes, ports);
    }

    private static void ValidateCounts(BoardConfig config)
    {
        // Expect exactly 19 hexes total for radius 2
        var expectedHexes = 3 * config.Radius * (config.Radius + 1) + 1; // 19
        var resourceSum = config.ResourceCounts.Values.Sum();
        if (resourceSum != expectedHexes)
        {
            throw new ArgumentException($"ResourceCounts sum {resourceSum} does not match expected hex count {expectedHexes}.");
        }

        // Exactly 18 number tokens (all non-desert hexes)
        var nonDesert = resourceSum - (config.ResourceCounts.TryGetValue(ResourceCardType.Desert, out var desert) ? desert : 0);
        if (nonDesert != 18)
        {
            throw new ArgumentException("StandardBoardGenerator expects exactly one desert tile.");
        }
        if (config.NumberTokens.Count != nonDesert)
        {
            throw new ArgumentException($"NumberTokens count {config.NumberTokens.Count} does not match non-desert hexes {nonDesert}.");
        }

        // Optionally ensure the multiset of tokens match the standard multiset
        var sortedCfg = config.NumberTokens.OrderBy(x => x).ToArray();
        var sortedStd = s_standardNumberTokenSequence.OrderBy(x => x).ToArray();
        if (!sortedCfg.SequenceEqual(sortedStd))
        {
            throw new ArgumentException("Provided NumberTokens do not match the standard base game multiset.");
        }
    }

    private static List<ResourceCardType> BuildDeterministicResourceSequence()
    {
        // Build sequence by repeating a balanced cycle to meet exact counts.
        // Cycle: Lumber, Wool, Grain, Brick, Ore (x3) + Lumber, Wool, Grain
        // Totals: L4, W4, G4, B3, O3
        var seq = new List<ResourceCardType>(capacity: 18);
        for (int i = 0; i < 3; i++)
        {
            seq.Add(ResourceCardType.Lumber);
            seq.Add(ResourceCardType.Wool);
            seq.Add(ResourceCardType.Grain);
            seq.Add(ResourceCardType.Brick);
            seq.Add(ResourceCardType.Ore);
        }
        seq.Add(ResourceCardType.Lumber);
        seq.Add(ResourceCardType.Wool);
        seq.Add(ResourceCardType.Grain);
        return seq;
    }

    // The classic Catan chit sequence (outer ring 12, then inner ring 6) skipping desert
    private static readonly int[] s_standardNumberTokenSequence =
    [
        5, 2, 6, 3, 8, 10, 9, 12, 11, 4, 8, 10, // outer ring (12)
        9, 4, 5, 6, 3, 11                      // inner ring (6)
    ];

    private static IEnumerable<Edge> GetBorderEdges(IEnumerable<HexCoord> coords)
    {
        var set = new HashSet<HexCoord>(coords);
        foreach (var h in set)
        {
            foreach (var neighbor in h.GetAdjacentHexCoords().Values)
            {
                if (!set.Contains(neighbor))
                    yield return new Edge(h, neighbor).Normalize();
            }
        }
    }

    private static IEnumerable<HexCoord> GetHexCoords(int radius)
    {
        for (int q = -radius; q <= radius; q++)
        {
            int r1 = Math.Max(-radius, -q - radius);
            int r2 = Math.Min(radius, -q + radius);
            for (int r = r1; r <= r2; r++)
            {
                int s = -q - r; // cube coordinate third component
                yield return new HexCoord(q, r, s);
            }
        }
    }

    private static List<HexCoord> GetRing(HexCoord center, int radius)
    {
        if (radius <= 0) return [];

        // Direction vectors in cube coordinates
        var north = new HexCoord(0, -1, +1);
        var northEast = new HexCoord(+1, -1, 0);
        var southEast = new HexCoord(+1, 0, -1);
        var south = new HexCoord(0, +1, -1);
        var southWest = new HexCoord(-1, +1, 0);
        var northWest = new HexCoord(-1, 0, +1);

        // Start at center + north * radius (top of the ring)
        var cur = Add(center, Scale(north, radius));

        var ring = new List<HexCoord>(capacity: 6 * radius);

        // Walk around the ring clockwise: NE -> SE -> S -> SW -> NW -> N
        var dirs = new[] { northEast, southEast, south, southWest, northWest, north };
        foreach (var dir in dirs)
        {
            for (int i = 0; i < radius; i++)
            {
                cur = Add(cur, dir);
                ring.Add(cur);
            }
        }
        return ring;
    }

    private static HexCoord Add(HexCoord a, HexCoord b) => new(a.Q + b.Q, a.R + b.R, a.S + b.S);
    private static HexCoord Scale(HexCoord a, int k) => new(a.Q * k, a.R * k, a.S * k);

    private static List<PortType> BuildDeterministicPortTypes(IReadOnlyDictionary<PortType, int> ports)
    {
        // Assign the specific resource ports first in a fixed order, then fill the remainder with Generic3to1
        var ordered = new List<PortType>();

        void AddMany(PortType t)
        {
            if (ports.TryGetValue(t, out var c) && c > 0)
            {
                for (int i = 0; i < c; i++) ordered.Add(t);
            }
        }

        AddMany(PortType.Brick2to1);
        AddMany(PortType.Lumber2to1);
        AddMany(PortType.Wool2to1);
        AddMany(PortType.Grain2to1);
        AddMany(PortType.Ore2to1);
        AddMany(PortType.Generic3to1);

        return ordered;
    }
}
