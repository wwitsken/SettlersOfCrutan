using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Generation;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Domain.Lobbies;
using SettlersOfCrutan.Domain.Users;

namespace SettlersOfCrutan.Domain.UnitTests;

public class GameBuildableQueriesTests
{
    private static UserId User(int seed) =>
        UserId.Create(Guid.Parse($"{seed & 0xFFFFFFFF:X8}-0000-0000-0000-000000000000"));

    [Fact]
    public void GetBuildableRoads_ReturnsOnlyEdgesConnectedToPlayersNetwork()
    {
        var origin = new HexCoord(0, 0, 0);
        var board = Board.Create([new Hex(origin) { Resource = ResourceCardType.Desert }], []);

        var lobbyId = new LobbyId { Value = Guid.NewGuid() };
        var created = Game.CreateGame("test", lobbyId, [User(1), User(2)], new RandomBoardGenerator());
        Assert.True(created.IsSuccess);
        var game = created.Value;
        var p1 = game.Players[0].Id;
        var p2 = game.Players[1].Id;

        var hex = new HexCoord(0, 0, 0);
        var v0 = VertexFactory.FromHexCorner(hex, HexCornerDirection.NE);
        var seedEdge = new Edge(v0.HexCoord1, v0.HexCoord2).Normalize();

        var placed = board.PlaceInitialSettlementAndRoad(p1, v0, seedEdge);
        Assert.True(placed.IsSuccess);

        game.Board = board;
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

        var lobbyId = new LobbyId { Value = Guid.NewGuid() };
        var created = Game.CreateGame("test", lobbyId, [User(1)], new RandomBoardGenerator());
        Assert.True(created.IsSuccess);
        var game = created.Value;
        var p1 = game.Players[0].Id;

        var hex = new HexCoord(0, 0, 0);
        var v0 = VertexFactory.FromHexCorner(hex, HexCornerDirection.NE);
        var seedEdge = new Edge(v0.HexCoord1, v0.HexCoord2).Normalize();

        var placed = board.PlaceInitialSettlementAndRoad(p1, v0, seedEdge);
        Assert.True(placed.IsSuccess);

        game.Board = board;

        var buildable = game.GetBuildableSettlements(p1);

        Assert.DoesNotContain(buildable, v => v.Normalize().Equals(v0.Normalize()));

        var adjacent = VertexFactory.GetAdjacentVertices(v0).Select(a => a.Normalize()).ToHashSet();
        Assert.DoesNotContain(buildable, v => adjacent.Contains(v.Normalize()));

        Assert.All(buildable, v => Assert.True(board.CanPlaceSettlement(p1, v).IsSuccess));
    }
}
