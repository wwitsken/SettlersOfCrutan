using JoinSpecs = SettlersOfCrutan.Domain.Specifications.JoinPlayer;
using LeaveSpecs = SettlersOfCrutan.Domain.Specifications.LeavePlayer;
using ColorSpecs = SettlersOfCrutan.Domain.Specifications.PlayerSetColor;
using NameSpecs = SettlersOfCrutan.Domain.Specifications.PlayerSetName;
using ReadySpecs = SettlersOfCrutan.Domain.Specifications.PlayerSetReady;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Domain.UnitTests.Specifications;

public class EntrySpecTests
{
    private static readonly GameId GameId = new() { Value = Guid.NewGuid() };
    private static readonly PlayerId P1 = new() { Value = "p1" };

    [Fact]
    public void JoinPlayer_PlayerWithUserIdMustExist_PlayerPresent_Succeeds()
    {
        var ctx = new JoinSpecs.JoinPlayerContext(GameId, Player.Create(TestIds.User(1)));
        var result = new JoinSpecs.PlayerWithUserIdMustExist().IsSatisfiedBy(ctx);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void JoinPlayer_PlayerWithUserIdMustExist_NullPlayer_Fails()
    {
        var ctx = new JoinSpecs.JoinPlayerContext(GameId, null);
        var result = new JoinSpecs.PlayerWithUserIdMustExist().IsSatisfiedBy(ctx);
        Assert.True(result.IsFailure);
        Assert.Equal("UserNotInGame", result.Error.Code);
    }

    [Fact]
    public void LeavePlayer_PlayerMustExist_PlayerPresent_Succeeds()
    {
        var ctx = new LeaveSpecs.LeavePlayerContext(GameId, P1, Player.Create(TestIds.User(1)));
        var result = new LeaveSpecs.PlayerMustExist().IsSatisfiedBy(ctx);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void LeavePlayer_PlayerMustExist_NullPlayer_Fails()
    {
        var ctx = new LeaveSpecs.LeavePlayerContext(GameId, P1, null);
        var result = new LeaveSpecs.PlayerMustExist().IsSatisfiedBy(ctx);
        Assert.True(result.IsFailure);
        Assert.Equal("PlayerNotFound", result.Error.Code);
    }

    [Fact]
    public void PlayerSetName_PlayerMustExist_PlayerPresent_Succeeds()
    {
        var ctx = new NameSpecs.PlayerSetNameContext(GameId, P1, Player.Create(TestIds.User(1)));
        var result = new NameSpecs.PlayerMustExist().IsSatisfiedBy(ctx);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerSetName_PlayerMustExist_NullPlayer_Fails()
    {
        var ctx = new NameSpecs.PlayerSetNameContext(GameId, P1, null);
        var result = new NameSpecs.PlayerMustExist().IsSatisfiedBy(ctx);
        Assert.True(result.IsFailure);
        Assert.Equal("PlayerNotFound", result.Error.Code);
    }

    [Fact]
    public void PlayerSetColor_PlayerMustExist_PlayerPresent_Succeeds()
    {
        var player = Player.Create(TestIds.User(1));
        var ctx = new ColorSpecs.PlayerSetColorContext(GameId, player.Id, player, PlayerColor.Red, [player]);
        var result = new ColorSpecs.PlayerMustExist().IsSatisfiedBy(ctx);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerSetColor_PlayerMustExist_NullPlayer_Fails()
    {
        var ctx = new ColorSpecs.PlayerSetColorContext(GameId, P1, null, PlayerColor.Red, []);
        var result = new ColorSpecs.PlayerMustExist().IsSatisfiedBy(ctx);
        Assert.True(result.IsFailure);
        Assert.Equal("PlayerNotFound", result.Error.Code);
    }

    [Fact]
    public void PlayerSetColor_ColorMustNotBeNone_Red_Succeeds()
    {
        var player = Player.Create(TestIds.User(1));
        var ctx = new ColorSpecs.PlayerSetColorContext(GameId, player.Id, player, PlayerColor.Red, [player]);
        var result = new ColorSpecs.ColorMustNotBeNone().IsSatisfiedBy(ctx);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerSetColor_ColorMustNotBeNone_None_Fails()
    {
        var player = Player.Create(TestIds.User(1));
        var ctx = new ColorSpecs.PlayerSetColorContext(GameId, player.Id, player, PlayerColor.None, [player]);
        var result = new ColorSpecs.ColorMustNotBeNone().IsSatisfiedBy(ctx);
        Assert.True(result.IsFailure);
        Assert.Equal("InvalidColor", result.Error.Code);
    }

    [Fact]
    public void PlayerSetColor_ColorMustNotBeTaken_UniqueColor_Succeeds()
    {
        var a = Player.Create(TestIds.User(10));
        var b = Player.Create(TestIds.User(11));
        b.SetColor(PlayerColor.Red);
        var ctx = new ColorSpecs.PlayerSetColorContext(GameId, a.Id, a, PlayerColor.Blue, [a, b]);
        var result = new ColorSpecs.ColorMustNotBeTaken().IsSatisfiedBy(ctx);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerSetColor_ColorMustNotBeTaken_ColorTakenByOther_Fails()
    {
        var a = Player.Create(TestIds.User(10));
        var b = Player.Create(TestIds.User(11));
        b.SetColor(PlayerColor.Red);
        var ctx = new ColorSpecs.PlayerSetColorContext(GameId, a.Id, a, PlayerColor.Red, [a, b]);
        var result = new ColorSpecs.ColorMustNotBeTaken().IsSatisfiedBy(ctx);
        Assert.True(result.IsFailure);
        Assert.Equal("ColorTaken", result.Error.Code);
    }

    [Fact]
    public void PlayerSetReady_PlayerMustExist_PlayerPresent_Succeeds()
    {
        var player = Player.Create(TestIds.User(1));
        var ctx = new ReadySpecs.PlayerSetReadyContext(GameId, player.Id, player, false);
        var result = new ReadySpecs.PlayerMustExist().IsSatisfiedBy(ctx);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerSetReady_PlayerMustExist_NullPlayer_Fails()
    {
        var ctx = new ReadySpecs.PlayerSetReadyContext(GameId, P1, null, false);
        var result = new ReadySpecs.PlayerMustExist().IsSatisfiedBy(ctx);
        Assert.True(result.IsFailure);
        Assert.Equal("PlayerNotFound", result.Error.Code);
    }

    [Fact]
    public void PlayerSetReady_PlayerMustHaveColorBeforeReady_ColoredPlayer_ReadyTrue_Succeeds()
    {
        var player = Player.Create(TestIds.User(1));
        player.SetColor(PlayerColor.Green);
        player.SetName("Alice");
        var ctx = new ReadySpecs.PlayerSetReadyContext(GameId, player.Id, player, true);
        var result = new ReadySpecs.PlayerMustHaveColorBeforeReady().IsSatisfiedBy(ctx);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerSetReady_PlayerMustHaveColorBeforeReady_NoneColor_ReadyTrue_Fails()
    {
        var player = Player.Create(TestIds.User(1));
        player.SetName("Alice");
        var ctx = new ReadySpecs.PlayerSetReadyContext(GameId, player.Id, player, true);
        var result = new ReadySpecs.PlayerMustHaveColorBeforeReady().IsSatisfiedBy(ctx);
        Assert.True(result.IsFailure);
        Assert.Equal("ColorMustSetBeforeReady", result.Error.Code);
    }

    [Fact]
    public void PlayerSetReady_PlayerMustHaveNameBeforeReady_NamedPlayer_ReadyTrue_Succeeds()
    {
        var player = Player.Create(TestIds.User(1));
        player.SetColor(PlayerColor.Green);
        player.SetName("Alice");
        var ctx = new ReadySpecs.PlayerSetReadyContext(GameId, player.Id, player, true);
        var result = new ReadySpecs.PlayerMustHaveNameBeforeReady().IsSatisfiedBy(ctx);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayerSetReady_PlayerMustHaveNameBeforeReady_EmptyName_ReadyTrue_Fails()
    {
        var player = Player.Create(TestIds.User(1));
        player.SetColor(PlayerColor.Green);
        var ctx = new ReadySpecs.PlayerSetReadyContext(GameId, player.Id, player, true);
        var result = new ReadySpecs.PlayerMustHaveNameBeforeReady().IsSatisfiedBy(ctx);
        Assert.True(result.IsFailure);
        Assert.Equal("NameMustSetBeforeReady", result.Error.Code);
    }
}
