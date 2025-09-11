using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.UnitTests;
public class TransactionTests
{
    [Fact]
    public void Transaction_CanAddAndSubtractResourceAmounts()
    {
        var bankResources = ResourceHand.StandardBankResources();
        var oldBankResources = ResourceHand.StandardBankResources();
        var bankDevCards = DevCardHand.StandardBankDeck();
        var oldBankDevCards = DevCardHand.StandardBankDeck();

        // Apply resource card deltas
        bankResources.Apply(ResourceCardType.Ore, 1);
        bankResources.Apply(ResourceCardType.Wool, 1);
        bankResources.Apply(ResourceCardType.Grain, 1);

        // Apply dev card delta
        bankDevCards.Apply(DevelopmentCardType.Knight, -1);

        Assert.Equal(oldBankResources.Count(ResourceCardType.Grain) + 1, bankResources.Count(ResourceCardType.Grain));
        Assert.Equal(oldBankDevCards.Count(DevelopmentCardType.Knight) - 1, bankDevCards.Count(DevelopmentCardType.Knight));
    }
}

