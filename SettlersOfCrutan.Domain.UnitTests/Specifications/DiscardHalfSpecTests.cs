using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Domain.Specifications.DiscardHalf;

namespace SettlersOfCrutan.Domain.UnitTests.Specifications;

public class DiscardHalfSpecTests
{
    private static readonly PlayerId P1 = new() { Value = "p1" };

    private static DiscardHalfContext MakeContext(
        GamePhase phase = GamePhase.DiscardHalf,
        DiscardHalfRequirement? requirement = null,
        List<ResourceCardAmount>? discards = null,
        int discardTotal = 4,
        Player? player = null) =>
        new(phase, requirement, discards, discardTotal, player);

    private static DiscardHalfRequirement Req(int amount = 4) =>
        new() { PlayerId = P1, ResourceAmount = amount };

    [Fact]
    public void GameMustBeInDiscardHalfPhase_DiscardHalf_Succeeds()
    {
        var result = new GameMustBeInDiscardHalfPhase().IsSatisfiedBy(MakeContext(phase: GamePhase.DiscardHalf));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void GameMustBeInDiscardHalfPhase_TradeBuild_Fails()
    {
        var result = new GameMustBeInDiscardHalfPhase().IsSatisfiedBy(MakeContext(phase: GamePhase.TradeBuild));
        Assert.True(result.IsFailure);
        Assert.Equal("Discard", result.Error.Code);
    }

    [Fact]
    public void PlayerMustBeRequiredToDiscard_HasRequirement_Succeeds()
    {
        var result = new PlayerMustBeRequiredToDiscard().IsSatisfiedBy(MakeContext(requirement: Req()));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerMustBeRequiredToDiscard_NullRequirement_Fails()
    {
        var result = new PlayerMustBeRequiredToDiscard().IsSatisfiedBy(MakeContext(requirement: null));
        Assert.True(result.IsFailure);
        Assert.Equal("PlayerNotRequired", result.Error.Code);
    }

    [Fact]
    public void DiscardsMustBeValid_NonEmptyList_Succeeds()
    {
        var result = new DiscardsMustBeValid().IsSatisfiedBy(
            MakeContext(discards: [new ResourceCardAmount(ResourceCardType.Brick, 2)]));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void DiscardsMustBeValid_NullDiscards_Fails()
    {
        var result = new DiscardsMustBeValid().IsSatisfiedBy(MakeContext(discards: null));
        Assert.True(result.IsFailure);
        Assert.Equal("InvalidDiscardsPayload", result.Error.Code);
    }

    [Fact]
    public void DiscardsMustBeValid_EmptyList_Fails()
    {
        var result = new DiscardsMustBeValid().IsSatisfiedBy(MakeContext(discards: []));
        Assert.True(result.IsFailure);
        Assert.Equal("InvalidDiscardsPayload", result.Error.Code);
    }

    [Fact]
    public void DiscardAmountMustBeCorrect_MatchesRequirement_Succeeds()
    {
        var result = new DiscardAmountMustBeCorrect().IsSatisfiedBy(
            MakeContext(requirement: Req(5), discardTotal: 5));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void DiscardAmountMustBeCorrect_WrongTotal_Fails()
    {
        var result = new DiscardAmountMustBeCorrect().IsSatisfiedBy(
            MakeContext(requirement: Req(5), discardTotal: 3));
        Assert.True(result.IsFailure);
        Assert.Equal("IncorrectDiscardAmount", result.Error.Code);
    }

    [Fact]
    public void PlayerMustExist_PlayerPresent_Succeeds()
    {
        var result = new PlayerMustExist().IsSatisfiedBy(MakeContext(player: Player.Create("u1")));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerMustExist_NullPlayer_Fails()
    {
        var result = new PlayerMustExist().IsSatisfiedBy(MakeContext(player: null));
        Assert.True(result.IsFailure);
        Assert.Equal("NotFound", result.Error.Code);
    }

    [Fact]
    public void PlayerMustHaveResourcesToDiscard_PlayerHasResources_Succeeds()
    {
        var player = Player.Create("u1");
        player.AddResource(ResourceCardType.Brick, 2);
        player.AddResource(ResourceCardType.Lumber, 2);
        var discards = new List<ResourceCardAmount> { new(ResourceCardType.Brick, 1), new(ResourceCardType.Lumber, 1) };
        var result = new PlayerMustHaveResourcesToDiscard().IsSatisfiedBy(
            MakeContext(player: player, discards: discards));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerMustHaveResourcesToDiscard_PlayerLacksResources_Fails()
    {
        var player = Player.Create("u1");
        player.AddResource(ResourceCardType.Brick, 1);
        var discards = new List<ResourceCardAmount> { new(ResourceCardType.Brick, 3) };
        var result = new PlayerMustHaveResourcesToDiscard().IsSatisfiedBy(
            MakeContext(player: player, discards: discards));
        Assert.True(result.IsFailure);
        Assert.Equal("InsufficientResources", result.Error.Code);
    }
}
