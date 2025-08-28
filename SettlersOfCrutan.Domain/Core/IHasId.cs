namespace SettlersOfCrutan.Domain.Core;
public interface IHasId<TId>
{
    TId Id { get; init; }
};
