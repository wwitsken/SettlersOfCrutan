using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.UnitTests;

public class PlayerPlayedDevelopmentCardsTests
{
    [Fact]
    public void KnightsPlayed_IsReturnedForKnight()
    {
        var p = Player.Create("u1");
        p.IncrementKnightsPlayed();
        p.IncrementKnightsPlayed();
        Assert.Equal(2, p.GetPlayedDevelopmentCardCount(DevelopmentCardType.Knight));
    }

    [Fact]
    public void RecordNonKnight_IncrementsMonopoly()
    {
        var p = Player.Create("u1");
        p.RecordNonKnightDevelopmentCardPlayed(DevelopmentCardType.Monopoly);
        p.RecordNonKnightDevelopmentCardPlayed(DevelopmentCardType.Monopoly);
        Assert.Equal(2, p.GetPlayedDevelopmentCardCount(DevelopmentCardType.Monopoly));
        Assert.Equal(0, p.GetPlayedDevelopmentCardCount(DevelopmentCardType.Knight));
    }

    [Fact]
    public void VictoryPointPlayedCount_IsAlwaysZero()
    {
        var p = Player.Create("u1");
        p.AddDevCard(DevelopmentCardType.VictoryPoint, 2);
        Assert.Equal(0, p.GetPlayedDevelopmentCardCount(DevelopmentCardType.VictoryPoint));
    }

    [Fact]
    public void RecordNonKnight_RejectsKnightAndVictoryPoint()
    {
        var p = Player.Create("u1");
        Assert.Throws<ArgumentException>(() =>
            p.RecordNonKnightDevelopmentCardPlayed(DevelopmentCardType.Knight));
        Assert.Throws<ArgumentException>(() =>
            p.RecordNonKnightDevelopmentCardPlayed(DevelopmentCardType.VictoryPoint));
    }
}
