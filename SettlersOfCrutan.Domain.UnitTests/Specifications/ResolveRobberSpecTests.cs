using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Domain.Specifications.ResolveRobber;

namespace SettlersOfCrutan.Domain.UnitTests.Specifications;

public class ResolveRobberSpecTests
{
    private static readonly HexCoord Origin = new(0, 0, 0);
    private static readonly PlayerId P1 = new() { Value = "p1" };
    private static readonly PlayerId P2 = new() { Value = "p2" };

    private static Board DesertBoard() =>
        Board.Create(
            [new Hex(Origin) { Resource = ResourceCardType.Desert }],
            []);

    private static ResolveRobberContext MakeContext(
        GamePhase phase = GamePhase.ResolveRobber,
        PlayerId? currentPlayerId = null,
        PlayerId? actingPlayerId = null,
        Board? board = null,
        HexCoord? newRobberHex = null,
        PlayerId? victimId = null,
        IReadOnlyList<PlayerId>? stealTargets = null) =>
        new(
            phase,
            currentPlayerId ?? P1,
            actingPlayerId ?? P1,
            board ?? DesertBoard(),
            newRobberHex ?? Origin,
            victimId,
            stealTargets ?? []);

    [Fact]
    public void GameMustBeInResolveRobberPhase_ResolveRobber_Succeeds()
    {
        var result = new GameMustBeInResolveRobberPhase().IsSatisfiedBy(MakeContext(phase: GamePhase.ResolveRobber));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void GameMustBeInResolveRobberPhase_TradeBuild_Fails()
    {
        var result = new GameMustBeInResolveRobberPhase().IsSatisfiedBy(MakeContext(phase: GamePhase.TradeBuild));
        Assert.True(result.IsFailure);
        Assert.Equal("Robber", result.Error.Code);
    }

    [Fact]
    public void MustBeCurrentPlayerTurn_SamePlayer_Succeeds()
    {
        var result = new MustBeCurrentPlayerTurn().IsSatisfiedBy(MakeContext(currentPlayerId: P1, actingPlayerId: P1));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void MustBeCurrentPlayerTurn_DifferentPlayer_Fails()
    {
        var result = new MustBeCurrentPlayerTurn().IsSatisfiedBy(
            MakeContext(currentPlayerId: P1, actingPlayerId: P2));
        Assert.True(result.IsFailure);
        Assert.Equal("WrongTurn", result.Error.Code);
    }

    [Fact]
    public void RobberDestinationMustBeValid_HexOnBoard_Succeeds()
    {
        var result = new RobberDestinationMustBeValid().IsSatisfiedBy(
            MakeContext(board: DesertBoard(), newRobberHex: Origin));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void RobberDestinationMustBeValid_HexNotOnBoard_Fails()
    {
        var result = new RobberDestinationMustBeValid().IsSatisfiedBy(
            MakeContext(board: new Board(), newRobberHex: Origin));
        Assert.True(result.IsFailure);
        Assert.Equal("Robber", result.Error.Code);
    }

    [Fact]
    public void RobberVictimMustBeValid_NoTargets_NoVictim_Succeeds()
    {
        var result = new RobberVictimMustBeValid().IsSatisfiedBy(
            MakeContext(victimId: null, stealTargets: Array.Empty<PlayerId>()));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void RobberVictimMustBeValid_NoTargets_WithVictim_Fails()
    {
        var result = new RobberVictimMustBeValid().IsSatisfiedBy(
            MakeContext(victimId: P2, stealTargets: Array.Empty<PlayerId>()));
        Assert.True(result.IsFailure);
        Assert.Equal("Robber", result.Error.Code);
    }

    [Fact]
    public void RobberVictimMustBeValid_TargetsExist_VictimInTargets_Succeeds()
    {
        var result = new RobberVictimMustBeValid().IsSatisfiedBy(
            MakeContext(victimId: P2, stealTargets: new[] { P2 }));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void RobberVictimMustBeValid_TargetsExist_NoVictim_Fails()
    {
        var result = new RobberVictimMustBeValid().IsSatisfiedBy(
            MakeContext(victimId: null, stealTargets: new[] { P2 }));
        Assert.True(result.IsFailure);
        Assert.Equal("Robber", result.Error.Code);
    }

    [Fact]
    public void RobberVictimMustBeValid_TargetsExist_VictimNotInTargets_Fails()
    {
        var p3 = new PlayerId { Value = "p3" };
        var result = new RobberVictimMustBeValid().IsSatisfiedBy(
            MakeContext(victimId: P2, stealTargets: new[] { p3 }));
        Assert.True(result.IsFailure);
        Assert.Equal("Robber", result.Error.Code);
    }
}
