using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Domain.Specifications.BuildCity;

namespace SettlersOfCrutan.Domain.UnitTests.Specifications;

public class BuildCitySpecTests
{
    private static readonly HexCoord Origin = new(0, 0, 0);
    private static readonly Vertex ValidVertex = VertexFactory.FromHexCorner(Origin, HexCornerDirection.NE);

    private static BuildCityContext MakeContext(
        GamePhase phase = GamePhase.TradeBuild,
        PlayerId? currentPlayerId = null,
        PlayerId? actingPlayerId = null,
        Player? player = null,
        List<ResourceCardAmount>? cost = null,
        Board? board = null,
        Vertex? vertex = null)
    {
        var pid = actingPlayerId ?? new PlayerId { Value = "p1" };
        return new BuildCityContext(
            phase,
            currentPlayerId ?? pid,
            pid,
            player ?? Player.Create(TestIds.User(1)),
            cost ?? [],
            board ?? new Board(),
            vertex ?? ValidVertex);
    }

    [Fact]
    public void GameMustBeInTradeBuildPhase_TradeBuild_Succeeds()
    {
        var result = new GameMustBeInTradeBuildPhase().IsSatisfiedBy(MakeContext());
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void GameMustBeInTradeBuildPhase_RollDice_Fails()
    {
        var result = new GameMustBeInTradeBuildPhase().IsSatisfiedBy(MakeContext(phase: GamePhase.RollDice));
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void MustBeCurrentPlayerTurn_SamePlayer_Succeeds()
    {
        var pid = new PlayerId { Value = "p1" };
        var result = new MustBeCurrentPlayerTurn().IsSatisfiedBy(MakeContext(currentPlayerId: pid, actingPlayerId: pid));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void MustBeCurrentPlayerTurn_DifferentPlayer_Fails()
    {
        var result = new MustBeCurrentPlayerTurn().IsSatisfiedBy(
            MakeContext(currentPlayerId: new PlayerId { Value = "p1" }, actingPlayerId: new PlayerId { Value = "p2" }));
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void PlayerMustAffordCity_HasResources_Succeeds()
    {
        var player = Player.Create(TestIds.User(1));
        player.AddResource(ResourceCardType.Ore, 3);
        player.AddResource(ResourceCardType.Grain, 2);
        var cost = new List<ResourceCardAmount> { new(ResourceCardType.Ore, 3), new(ResourceCardType.Grain, 2) };

        var result = new PlayerMustAffordCity().IsSatisfiedBy(MakeContext(player: player, cost: cost));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerMustAffordCity_NoResources_Fails()
    {
        var player = Player.Create(TestIds.User(1));
        var cost = new List<ResourceCardAmount> { new(ResourceCardType.Ore, 3) };

        var result = new PlayerMustAffordCity().IsSatisfiedBy(MakeContext(player: player, cost: cost));
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void PlayerMustHaveCityPiece_HasPieces_Succeeds()
    {
        var result = new PlayerMustHaveCityPiece().IsSatisfiedBy(MakeContext());
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerMustHaveCityPiece_NoPieces_Fails()
    {
        var player = Player.Create(TestIds.User(1));
        for (int i = 0; i < 4; i++) player.ConsumePiece(BuildableType.City);

        var result = new PlayerMustHaveCityPiece().IsSatisfiedBy(MakeContext(player: player));
        Assert.True(result.IsFailure);
        Assert.Equal("MissingCity", result.Error.Code);
    }

    [Fact]
    public void BoardMustAllowCityUpgrade_HasSettlement_Succeeds()
    {
        var board = new Board();
        var pid = new PlayerId { Value = "p1" };
        var seedEdge = VertexFactory.IncidentRoadEdges(ValidVertex).First();
        board.PlaceInitialSettlementAndRoad(pid, ValidVertex, seedEdge);

        var result = new BoardMustAllowCityUpgrade().IsSatisfiedBy(
            MakeContext(board: board, actingPlayerId: pid, vertex: ValidVertex));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void BoardMustAllowCityUpgrade_NoSettlement_Fails()
    {
        var board = new Board();
        var pid = new PlayerId { Value = "p1" };

        var result = new BoardMustAllowCityUpgrade().IsSatisfiedBy(
            MakeContext(board: board, actingPlayerId: pid, vertex: ValidVertex));
        Assert.True(result.IsFailure);
    }
}
