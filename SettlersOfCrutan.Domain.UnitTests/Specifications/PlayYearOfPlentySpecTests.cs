using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Domain.Specifications.PlayYearOfPlenty;

namespace SettlersOfCrutan.Domain.UnitTests.Specifications;

public class PlayYearOfPlentySpecTests
{
    private static readonly PlayerId P1 = new() { Value = "p1" };

    private static PlayYearOfPlentyContext MakeContext(
        GamePhase phase = GamePhase.TradeBuild,
        PlayerId? currentPlayerId = null,
        PlayerId? actingPlayerId = null,
        Player? actingPlayer = null,
        ResourceCardType? type1 = null,
        ResourceCardType? type2 = null,
        ResourceHand? bank = null) =>
        new(
            phase,
            currentPlayerId ?? P1,
            actingPlayerId ?? P1,
            actingPlayer ?? Player.Create(TestIds.User(1)),
            type1 ?? ResourceCardType.Brick,
            type2 ?? ResourceCardType.Lumber,
            bank ?? ResourceHand.StandardBankResources());

    [Fact]
    public void GameMustBeInTradeBuildPhase_TradeBuild_Succeeds()
    {
        var result = new GameMustBeInTradeBuildPhase().IsSatisfiedBy(MakeContext(phase: GamePhase.TradeBuild));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void GameMustBeInTradeBuildPhase_Setup_Fails()
    {
        var result = new GameMustBeInTradeBuildPhase().IsSatisfiedBy(MakeContext(phase: GamePhase.Setup));
        Assert.True(result.IsFailure);
        Assert.Equal("WrongGamePhase", result.Error.Code);
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
            MakeContext(currentPlayerId: P1, actingPlayerId: new PlayerId { Value = "p2" }));
        Assert.True(result.IsFailure);
        Assert.Equal("WrongTurn", result.Error.Code);
    }

    [Fact]
    public void PlayerMustHaveYearOfPlentyCard_HasCard_Succeeds()
    {
        var player = Player.Create(TestIds.User(1));
        player.AddDevCard(DevelopmentCardType.YearOfPlenty);
        var result = new PlayerMustHaveYearOfPlentyCard().IsSatisfiedBy(MakeContext(actingPlayer: player));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerMustHaveYearOfPlentyCard_NoCard_Fails()
    {
        var result = new PlayerMustHaveYearOfPlentyCard().IsSatisfiedBy(MakeContext(actingPlayer: Player.Create(TestIds.User(1))));
        Assert.True(result.IsFailure);
        Assert.Equal("DevCard", result.Error.Code);
    }

    [Fact]
    public void BankMustHaveRequestedResources_SameType_BankStocked_Succeeds()
    {
        var result = new BankMustHaveRequestedResources().IsSatisfiedBy(
            MakeContext(
                type1: ResourceCardType.Ore,
                type2: ResourceCardType.Ore,
                bank: ResourceHand.StandardBankResources()));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void BankMustHaveRequestedResources_SameType_EmptyBank_Fails()
    {
        var result = new BankMustHaveRequestedResources().IsSatisfiedBy(
            MakeContext(
                type1: ResourceCardType.Brick,
                type2: ResourceCardType.Brick,
                bank: new ResourceHand()));
        Assert.True(result.IsFailure);
        Assert.Equal("InsufficientResources", result.Error.Code);
    }

    [Fact]
    public void BankMustHaveRequestedResources_DifferentTypes_BankStocked_Succeeds()
    {
        var result = new BankMustHaveRequestedResources().IsSatisfiedBy(
            MakeContext(
                type1: ResourceCardType.Wool,
                type2: ResourceCardType.Grain,
                bank: ResourceHand.StandardBankResources()));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void BankMustHaveRequestedResources_DifferentTypes_BankMissingOne_Fails()
    {
        var bank = new ResourceHand();
        bank.Add(ResourceCardType.Brick, 5);
        var result = new BankMustHaveRequestedResources().IsSatisfiedBy(
            MakeContext(
                type1: ResourceCardType.Brick,
                type2: ResourceCardType.Lumber,
                bank: bank));
        Assert.True(result.IsFailure);
        Assert.Equal("InsufficientResources", result.Error.Code);
    }
}
