namespace SettlersOfCrutan.Domain.Core;
public interface IHasId<TId> where TId : BaseId { TId Id { get; } };
