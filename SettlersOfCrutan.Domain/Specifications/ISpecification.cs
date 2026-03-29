using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Specifications;

public interface ISpecification<TContext>
{
    Result<Nothing> IsSatisfiedBy(TContext context);
}
