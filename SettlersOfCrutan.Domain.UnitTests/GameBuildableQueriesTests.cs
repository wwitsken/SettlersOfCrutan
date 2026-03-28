using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Generation;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Domain.UnitTests;

public class GameBuildableQueriesTests
{
    private static PlayerId NewPlayer(string id) => new() { Value = id };

    private static Game NewGameWithBoard(Board board, params string[] userIds)
    {
        var lobbyId = new LobbyId { Value = Guid.NewGuid() };
        var created = Game.CreateGame("test", lobbyId, userIds, new RandomBoardGenerator());
        Assert.True(created.IsSuccess);

        var game = created.Value;
        game.Board = board;
        return game;
    }

    [Fact]
    public void GetBuildableRoads_ReturnsOnlyEdgesConnectedToPlayersNetwork()
    {
        var origin = new HexCoord(0, 0, 0);
        var board = Board.Create([new Hex(origin) { Resource = ResourceCardType.Desert }], []);
        var p1 = NewPlayer("p1");
        var p2 = NewPlayer("p2");

        var hex = new HexCoord(0, 0, 0);
        var v0 = VertexFactory.FromHexCorner(hex, HexCornerDirection.NE);
        var seedEdge = new Edge(v0.HexCoord1, v0.HexCoord2).Normalize();

        var placed = board.PlaceInitialSettlementAndRoad(p1, v0, seedEdge);
        Assert.True(placed.IsSuccess);

        var game = NewGameWithBoard(board, p1.Value, p2.Value);
        game.GamePhase = GamePhase.TradeBuild;

        var buildable = game.GetBuildableRoads(p1);

        Assert.NotEmpty(buildable);
        Assert.All(buildable, e => Assert.True(board.CanPlaceRoad(p1, e).IsSuccess));

        Assert.Empty(game.GetBuildableRoads(p2));
    }

    [Fact]
    public void GetBuildableSettlements_ExcludesOccupiedAndAdjacentVertices()
    {
        var board = new Board();
        var p1 = NewPlayer("p1");

        var hex = new HexCoord(0, 0, 0);
        var v0 = VertexFactory.FromHexCorner(hex, HexCornerDirection.NE);
        var seedEdge = new Edge(v0.HexCoord1, v0.HexCoord2).Normalize();

        var placed = board.PlaceInitialSettlementAndRoad(p1, v0, seedEdge);
        Assert.True(placed.IsSuccess);

        var game = NewGameWithBoard(board, p1.Value);

        var buildable = game.GetBuildableSettlements(p1);

        Assert.DoesNotContain(buildable, v => v.Normalize().Equals(v0.Normalize()));

        var adjacent = VertexFactory.GetAdjacentVertices(v0).Select(a => a.Normalize()).ToHashSet();
        Assert.DoesNotContain(buildable, v => adjacent.Contains(v.Normalize()));

        Assert.All(buildable, v => Assert.True(board.CanPlaceSettlement(p1, v).IsSuccess));
    }
}
