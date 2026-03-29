using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;

namespace SettlersOfCrutan.Domain.Specifications.AcceptTrade;

public class TradeOfferMustExistAndMatch : ISpecification<AcceptTradeContext>
{
    public Result<Nothing> IsSatisfiedBy(AcceptTradeContext context) =>
        context.CurrentTradeOffer is not null && context.CurrentTradeOffer.Id == context.TradeOfferId
            ? Result.Success()
            : Result.Failure(DomainError.NotFound);
}
