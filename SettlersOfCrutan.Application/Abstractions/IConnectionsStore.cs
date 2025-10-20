using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Application.Abstractions;

public interface IConnectionsStore<TId> where TId : BaseId
{
    Task AddAsync(TId id, string userId, TimeSpan? ttl = null, CancellationToken ct = default);
    Task RemoveAsync(TId id, string userId, CancellationToken ct = default);
    Task<IReadOnlyCollection<string>> GetAllMembersAsync(TId id, CancellationToken ct = default);
    Task<IReadOnlyCollection<TId>> GetAllGroupsForUserAsync(string userId, CancellationToken ct = default);
    Task<long> CountAsync(TId id, CancellationToken ct = default);
    Task<bool> IsMemberAsync(TId id, string userId, CancellationToken ct = default);
}