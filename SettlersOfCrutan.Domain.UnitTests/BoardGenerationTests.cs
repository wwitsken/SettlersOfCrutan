using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Generation;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.UnitTests;

public class BoardGenerationTests
{
    private static BoardConfig StandardConfigRadius2() => new
    (
        Radius: 2,
        ResourceCounts: new Dictionary<ResourceCardType, int>
        {
            [ResourceCardType.Brick] = 3,
            [ResourceCardType.Lumber] = 4,
            [ResourceCardType.Wool] = 4,
            [ResourceCardType.Grain] = 4,
            [ResourceCardType.Ore] = 3,
            [ResourceCardType.Desert] = 1,
        },
        NumberTokens: [2, 3, 3, 4, 4, 5, 5, 6, 6, 8, 8, 9, 9, 10, 10, 11, 11, 12],
        Ports: new Dictionary<PortType, int>
        {
            [PortType.Generic3to1] = 4,
            [PortType.Brick2to1] = 1,
            [PortType.Lumber2to1] = 1,
            [PortType.Wool2to1] = 1,
            [PortType.Grain2to1] = 1,
            [PortType.Ore2to1] = 1,
        },
        FixedPortEdges: StandardBoardConfigurations.DefaultBaseGamePortEdges
    );

    [Fact]
    public void GeneratesExpectedHexCountAndResources()
    {
        var gen = new RandomBoardGenerator();
        var cfg = StandardConfigRadius2();
        var board = gen.Generate(cfg, seed: 42);

        int expectedHexes = 3 * cfg.Radius * (cfg.Radius + 1) + 1; // 19 for radius 2
        Assert.Equal(expectedHexes, board.Hexes.Count);

        // Resource distribution matches config
        foreach (var kvp in cfg.ResourceCounts)
        {
            Assert.Equal(kvp.Value, board.Hexes.Count(h => h.Resource == kvp.Key));
        }

        // Robber exists on exactly one desert (if any deserts)
        var deserts = board.Hexes.Where(h => h.Resource == ResourceCardType.Desert).ToList();
        if (deserts.Count > 0) Assert.Single(deserts, h => h.HasRobber);
    }

    [Fact]
    public void AssignsAllTokensAndAvoidsAdjacentHighs()
    {
        var gen = new RandomBoardGenerator();
        var cfg = StandardConfigRadius2();
        var board = gen.Generate(cfg, seed: 1337);

        // All non-desert hexes get a token
        var nonDeserts = board.Hexes.Where(h => h.Resource != ResourceCardType.Desert).ToList();
        Assert.All(nonDeserts, h => Assert.True(h.NumberToken.HasValue));
        Assert.Equal(cfg.NumberTokens.Count, nonDeserts.Count);

        // No adjacent 6/8 tiles
        Dictionary<HexCoord, Hex> byCoord = board.Hexes.ToDictionary(h => h.Coordinate);
        foreach (var h in nonDeserts)
        {
            if (h.NumberToken is 6 or 8)
            {
                foreach (HexCoord coord in h.Coordinate.GetAdjacentHexCoords().Values)
                {
                    if (byCoord.TryGetValue(coord, out var neigh) && neigh.Resource != ResourceCardType.Desert)
                    {
                        Assert.False(neigh.NumberToken is 6 or 8, $"High token adjacency at {h.Coordinate} and {coord}");
                    }
                }
            }
        }
    }

    [Fact]
    public void PortsOnBorderAndCountsMatch()
    {
        var gen = new RandomBoardGenerator();
        var cfg = StandardConfigRadius2();
        var board = gen.Generate(cfg, seed: 2024);

        int totalPorts = cfg.Ports.Values.Sum();
        Assert.Equal(totalPorts, board.Ports.Count);

        // Each port must be on a border edge (neighbor hex missing)
        foreach (var port in board.Ports)
        {
            var e = port.EdgeCoordinate;
            IEnumerable<Hex> hexes = board.Hexes.Where(h => h.Coordinate.Equals(e.HexCoord1) || h.Coordinate.Equals(e.HexCoord2));
            Assert.Single(hexes); // exactly one of the two hexes is on the board
        }

        // Types distribution matches
        foreach (var (type, count) in cfg.Ports)
        {
            Assert.Equal(count, board.Ports.Count(p => p.Type == type));
        }
    }

    [Fact]
    public void RandomGenerator_KeepsStandardPortEdgesWhileShufflingTypes()
    {
        var gen = new RandomBoardGenerator();
        var cfg = StandardConfigRadius2();
        var expectedEdges = StandardBoardConfigurations.DefaultBaseGamePortEdges
            .Select(s => new Edge(s.LandHex, s.SeaHex).Normalize())
            .OrderBy(e => e.ToString())
            .ToList();

        var boardA = gen.Generate(cfg, seed: 11);
        var boardB = gen.Generate(cfg, seed: 77);

        var edgesA = boardA.Ports.Select(p => p.EdgeCoordinate.Normalize()).OrderBy(e => e.ToString()).ToList();
        var edgesB = boardB.Ports.Select(p => p.EdgeCoordinate.Normalize()).OrderBy(e => e.ToString()).ToList();

        Assert.Equal(expectedEdges, edgesA);
        Assert.Equal(expectedEdges, edgesB);
        Assert.NotEqual(
            boardA.Ports.Select(p => p.Type).ToList(),
            boardB.Ports.Select(p => p.Type).ToList());
    }

}
