using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Domain.Games;

public partial class Game
{
    public List<Edge> GetBuildableRoads(PlayerId playerId)
    {
        var candidates = new HashSet<Edge>();

        foreach (var h in Board.Hexes)
        {
            foreach (var dir in Enum.GetValues<EdgeDirection>())
            {
                candidates.Add(EdgeFactory.FromHexEdge(h.Coordinate, dir).Normalize());
            }
        }

        return [.. candidates
            .Where(e => Board.CanPlaceRoad(playerId, e, GamePhase == GamePhase.Setup).IsSuccess)
            .OrderBy(e => e.ToString())];
    }

    public List<Vertex> GetBuildableSettlements(PlayerId playerId)
    {
        var candidates = new HashSet<Vertex>();

        foreach (var h in Board.Hexes)
        {
            foreach (var v in VertexFactory.FromHex(h.Coordinate))
            {
                candidates.Add(v.Normalize());
            }
        }

        return [.. candidates
            .Where(v => Board.CanPlaceSettlement(playerId, v, GamePhase == GamePhase.Setup).IsSuccess)
            .OrderBy(v => v.ToString())];
    }
}
