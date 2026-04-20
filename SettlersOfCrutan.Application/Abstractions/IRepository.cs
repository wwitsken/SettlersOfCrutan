using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Application.Abstractions;
public interface IRepository<TAgg, TId>
    where TAgg : Entity<TId>
    where TId : BaseId
{
    Task<TAgg?> GetAsync(TId id, CancellationToken ct = default);
    Task<IReadOnlyList<TAgg>> GetManyAsync(IEnumerable<TId> ids, CancellationToken ct = default);
    Task<bool> SaveAsync(TAgg aggregate, CancellationToken ct = default);
    Task<bool> DeleteAsync(TId id, CancellationToken ct = default);
}
