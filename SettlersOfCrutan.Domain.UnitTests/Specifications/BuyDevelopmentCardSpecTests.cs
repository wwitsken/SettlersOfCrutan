using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Domain.Specifications.BuyDevelopmentCard;

namespace SettlersOfCrutan.Domain.UnitTests.Specifications;

public class BuyDevelopmentCardSpecTests
{
    private static BuyDevelopmentCardContext MakeContext(
        GamePhase phase = GamePhase.TradeBuild,
        PlayerId? currentPlayerId = null,
        PlayerId? actingPlayerId = null,
        Player? player = null,
        List<ResourceCardAmount>? cost = null,
        int playerDevCardCount = 0,
        int bankDevCardCount = 25)
    {
        var pid = actingPlayerId ?? new PlayerId { Value = "p1" };
        return new BuyDevelopmentCardContext(
            phase,
            currentPlayerId ?? pid,
            pid,
            player ?? Player.Create(TestIds.User(1)),
            cost ?? [],
            playerDevCardCount,
            bankDevCardCount);
    }

    [Fact]
    public void GameMustBeInTradeBuildPhase_TradeBuild_Succeeds()
    {
        var result = new GameMustBeInTradeBuildPhase().IsSatisfiedBy(MakeContext());
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void GameMustBeInTradeBuildPhase_Setup_Fails()
    {
        var result = new GameMustBeInTradeBuildPhase().IsSatisfiedBy(MakeContext(phase: GamePhase.Setup));
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
    public void PlayerMustAffordDevelopmentCard_HasResources_Succeeds()
    {
        var player = Player.Create(TestIds.User(1));
        player.AddResource(ResourceCardType.Ore, 1);
        player.AddResource(ResourceCardType.Wool, 1);
        player.AddResource(ResourceCardType.Grain, 1);
        var cost = new List<ResourceCardAmount>
        {
            new(ResourceCardType.Ore, 1), new(ResourceCardType.Wool, 1), new(ResourceCardType.Grain, 1)
        };

        var result = new PlayerMustAffordDevelopmentCard().IsSatisfiedBy(MakeContext(player: player, cost: cost));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerMustAffordDevelopmentCard_NoResources_Fails()
    {
        var player = Player.Create(TestIds.User(1));
        var cost = new List<ResourceCardAmount> { new(ResourceCardType.Ore, 1) };

        var result = new PlayerMustAffordDevelopmentCard().IsSatisfiedBy(MakeContext(player: player, cost: cost));
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void PlayerMustNotExceedDevCardLimit_Under_Succeeds()
    {
        var result = new PlayerMustNotExceedDevCardLimit().IsSatisfiedBy(MakeContext(playerDevCardCount: 3));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerMustNotExceedDevCardLimit_Over_Fails()
    {
        var result = new PlayerMustNotExceedDevCardLimit().IsSatisfiedBy(MakeContext(playerDevCardCount: 4));
        Assert.True(result.IsFailure);
        Assert.Equal("TooManyDevelopmentCards", result.Error.Code);
    }

    [Fact]
    public void BankMustHaveDevelopmentCards_HasCards_Succeeds()
    {
        var result = new BankMustHaveDevelopmentCards().IsSatisfiedBy(MakeContext(bankDevCardCount: 1));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void BankMustHaveDevelopmentCards_Empty_Fails()
    {
        var result = new BankMustHaveDevelopmentCards().IsSatisfiedBy(MakeContext(bankDevCardCount: 0));
        Assert.True(result.IsFailure);
        Assert.Equal("InsufficientBankDevelopmentCards", result.Error.Code);
    }
}
