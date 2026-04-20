using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Application.Games;

/// <summary>
/// Public-facing scoring for DTOs (building VP, road/army specials). Does not include hidden VP dev cards.
/// </summary>
public static class GamePresentationScoring
{
    public const int MinLongestRoadSegments = 5;
    public const int MinLargestArmyKnights = 3;
    public const int VictoryPointsLongestRoad = 2;
    public const int VictoryPointsLargestArmy = 2;

    /// <summary>
    /// Visible VP from the board and public bonuses (excludes hidden VP development cards).
    /// </summary>
    public static int ObservableVictoryPoints(
        int buildingVictoryPoints,
        bool hasLongestRoad,
        bool hasLargestArmy) =>
        buildingVictoryPoints
        + (hasLongestRoad ? VictoryPointsLongestRoad : 0)
        + (hasLargestArmy ? VictoryPointsLargestArmy : 0);

    public static Dictionary<PlayerId, int> BuildingVictoryPoints(Game game)
    {
        var dict = game.Players.ToDictionary(p => p.Id, _ => 0);
        foreach (var pc in game.Board.PopulationCenters)
        {
            if (dict.ContainsKey(pc.OwnerId))
                dict[pc.OwnerId] += (int)pc.Level;
        }
        return dict;
    }

    public static int LongestRoadLengthForPlayer(Game game, PlayerId owner)
    {
        var roads = game.Board.Roads.Where(r => r.OwnerId.Equals(owner)).ToList();
        if (roads.Count == 0) return 0;

        var adj = new Dictionary<Vertex, List<(Vertex Neighbor, Edge Edge)>>();
        foreach (var road in roads)
        {
            var e = road.EdgeCoordinate.Normalize();
            var (a, b) = VertexFactory.EndpointsForEdge(e);
            AddAdj(adj, a, b, e);
            AddAdj(adj, b, a, e);
        }

        int best = 0;
        foreach (var start in adj.Keys)
            best = Math.Max(best, DfsLongestRoad(start, adj, new HashSet<Edge>()));
        return best;

        static void AddAdj(Dictionary<Vertex, List<(Vertex, Edge)>> map, Vertex from, Vertex to, Edge edge)
        {
            if (!map.TryGetValue(from, out var list))
            {
                list = [];
                map[from] = list;
            }
            list.Add((to, edge));
        }

        static int DfsLongestRoad(Vertex cur, Dictionary<Vertex, List<(Vertex Neighbor, Edge Edge)>> map, HashSet<Edge> pathEdges)
        {
            if (!map.TryGetValue(cur, out var outs)) return 0;
            int max = 0;
            foreach (var (nv, e) in outs)
            {
                var en = e.Normalize();
                if (!pathEdges.Add(en)) continue;
                max = Math.Max(max, 1 + DfsLongestRoad(nv, map, pathEdges));
                pathEdges.Remove(en);
            }
            return max;
        }
    }

    public static Dictionary<PlayerId, int> LongestRoadLengths(Game game) =>
        game.Players.ToDictionary(p => p.Id, p => LongestRoadLengthForPlayer(game, p.Id));

    /// <summary>
    /// Players tied for max length at least <see cref="MinLongestRoadSegments"/> earn the longest-road display.
    /// </summary>
    public static HashSet<PlayerId> LongestRoadHolders(Game game)
    {
        var lengths = LongestRoadLengths(game);
        var max = lengths.Values.DefaultIfEmpty(0).Max();
        if (max < MinLongestRoadSegments) return [];
        return [.. lengths.Where(kvp => kvp.Value == max).Select(kvp => kvp.Key)];
    }

    /// <summary>
    /// Players tied for most knights played at least <see cref="MinLargestArmyKnights"/>.
    /// </summary>
    public static HashSet<PlayerId> LargestArmyHolders(Game game)
    {
        var counts = game.Players.ToDictionary(p => p.Id, p => p.KnightsPlayed);
        var max = counts.Values.DefaultIfEmpty(0).Max();
        if (max < MinLargestArmyKnights) return [];
        return [.. counts.Where(kvp => kvp.Value == max).Select(kvp => kvp.Key)];
    }
}
