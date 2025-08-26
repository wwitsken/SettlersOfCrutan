using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Generation;

public class RandomBoardGenerator : IBoardGenerator
{
    public Board Generate(BoardConfig config, int seed)
    {
        ValidateConfig(config);
        var rng = new Random(seed);

        var board = new Board
        {
            Hexes = [],
            Vertices = [],
            Edges = [],
            Roads = [],
            Settlements = [],
            Cities = [],
            Ports = []
        };

        // Build axial coordinates within radius
        var coords = GetAxialCoords(config.Radius).ToList();
        var hexCount = coords.Count;

        // Assign resources according to counts
        var resourceBag = BuildResourceBag(config.ResourceCounts, hexCount);
        Shuffle(resourceBag, rng);

        // Assign resources to hexes
        var hexes = new List<Hex>(hexCount);
        for (int i = 0; i < hexCount; i++)
        {
            var h = new Hex
            {
                BoardId = board.Id,
                Coord = coords[i],
                Resource = resourceBag[i],
                NumberToken = null,
                HasRobber = false
            };
            hexes.Add(h);
        }

        // Place robber on the first desert if any
        var desertHex = hexes.FirstOrDefault(x => x.Resource == ResourceType.Desert);
        if (desertHex is not null) desertHex.HasRobber = true;

        // Assign number tokens to non-desert hexes with 6/8 adjacency constraint
        var numberTokens = config.NumberTokens.ToList();
        if (numberTokens.Count != hexes.Count(h => h.Resource != ResourceType.Desert))
        {
            // best-effort: if mismatch, we will only fill up to min count
            numberTokens = numberTokens.Take(hexes.Count(h => h.Resource != ResourceType.Desert)).ToList();
        }

        AssignNumberTokens(hexes, numberTokens, rng);

        board.Hexes = hexes;

        // Generate random ports on border edges
        var totalPorts = config.Ports.Sum(p => p.count);
        var borderEdges = GetBorderEdges(coords).ToList();
        Shuffle(borderEdges, rng);
        var chosenEdges = borderEdges.Take(totalPorts).ToList();

        var portTypes = new List<PortType>();
        foreach (var (type, count) in config.Ports)
        {
            for (int i = 0; i < count; i++) portTypes.Add(type);
        }
        Shuffle(portTypes, rng);
        var ports = new List<Port>();
        for (int i = 0; i < Math.Min(portTypes.Count, chosenEdges.Count); i++)
        {
            ports.Add(new Port { BoardId = board.Id, Type = portTypes[i], Edge = chosenEdges[i] });
        }
        board.Ports = ports;

        return board;
    }

    private static void ValidateConfig(BoardConfig config)
    {
        if (config.Radius < 0) throw new ArgumentOutOfRangeException(nameof(config.Radius));
        var expectedHexes = 3 * config.Radius * (config.Radius + 1) + 1;
        var resourceSum = config.ResourceCounts.Values.Sum();
        if (resourceSum != expectedHexes)
        {
            throw new ArgumentException($"ResourceCounts sum {resourceSum} does not match expected hex count {expectedHexes}.");
        }
        var nonDesert = resourceSum - (config.ResourceCounts.TryGetValue(ResourceType.Desert, out var desert) ? desert : 0);
        if (config.NumberTokens.Count != nonDesert)
        {
            // allow mismatch but warn via exception for generator correctness
            // consumers can catch and adjust if they want best-effort
            throw new ArgumentException($"NumberTokens count {config.NumberTokens.Count} does not match non-desert hexes {nonDesert}.");
        }
    }

    private static List<ResourceType> BuildResourceBag(IReadOnlyDictionary<ResourceType, int> counts, int expected)
    {
        var bag = new List<ResourceType>(expected);
        foreach (var kvp in counts)
        {
            for (int i = 0; i < kvp.Value; i++) bag.Add(kvp.Key);
        }
        if (bag.Count != expected)
        {
            throw new ArgumentException("ResourceCounts total does not equal expected hex count.");
        }
        return bag;
    }

    private static IEnumerable<AxialCoord> GetAxialCoords(int radius)
    {
        for (int q = -radius; q <= radius; q++)
        {
            int r1 = Math.Max(-radius, -q - radius);
            int r2 = Math.Min(radius, -q + radius);
            for (int r = r1; r <= r2; r++)
            {
                yield return new AxialCoord(q, r);
            }
        }
    }

    private static void Shuffle<T>(IList<T> list, Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static void AssignNumberTokens(List<Hex> hexes, List<int> tokens, Random rng)
    {
        var nonDesertIndexes = hexes.Select((h, idx) => (h, idx)).Where(x => x.h.Resource != ResourceType.Desert).Select(x => x.idx).ToList();
        var high = tokens.Where(t => t == 6 || t == 8).ToList();
        var others = tokens.Where(t => t != 6 && t != 8).ToList();

        // adjacency map among non-desert indices
        var indexByCoord = hexes.ToDictionary(h => h.Coord, h => h);
        bool IsAdjacentWithHigh(int idx)
        {
            var h = hexes[idx];
            foreach (EdgeDirection dir in Enum.GetValues(typeof(EdgeDirection)))
            {
                var neighborCoord = HexTopology.Neighbor(h.Coord, dir);
                if (indexByCoord.TryGetValue(neighborCoord, out var n))
                {
                    if (n.NumberToken is 6 or 8) return true;
                }
            }
            return false;
        }

        // reset
        foreach (var i in nonDesertIndexes) hexes[i].NumberToken = null;

        // Place high tokens greedily avoiding adjacency
        Shuffle(nonDesertIndexes, rng);
        foreach (var t in high)
        {
            bool placed = false;
            foreach (var idx in nonDesertIndexes)
            {
                if (hexes[idx].NumberToken.HasValue) continue;
                if (IsAdjacentWithHigh(idx)) continue;
                hexes[idx].NumberToken = t;
                placed = true; break;
            }
            if (!placed)
            {
                // fallback: place anywhere remaining
                var idx = nonDesertIndexes.First(i => !hexes[i].NumberToken.HasValue);
                hexes[idx].NumberToken = t;
            }
        }
        // Place remaining tokens randomly
        Shuffle(others, rng);
        foreach (var t in others)
        {
            var idx = nonDesertIndexes.First(i => !hexes[i].NumberToken.HasValue);
            hexes[idx].NumberToken = t;
        }
    }

    private static IEnumerable<EdgeCoord> GetBorderEdges(IEnumerable<AxialCoord> coords)
    {
        var set = new HashSet<AxialCoord>(coords);
        foreach (var h in set)
        {
            foreach (EdgeDirection dir in Enum.GetValues(typeof(EdgeDirection)))
            {
                var n = HexTopology.Neighbor(h, dir);
                if (!set.Contains(n))
                {
                    yield return new EdgeCoord(h, dir);
                }
            }
        }
    }
}
