using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Specifications.ProposeTrade;

public class NoActiveTradeOffer : ISpecification<ProposeTradeContext>
{
    public Result<Nothing> IsSatisfiedBy(ProposeTradeContext context) =>
        context.CurrentTradeOffer is null || context.CurrentTradeOffer.IsAccepted
            ? Result.Success()
            : Result.Failure(DomainError.AnotherTradeOfferActive);
}
