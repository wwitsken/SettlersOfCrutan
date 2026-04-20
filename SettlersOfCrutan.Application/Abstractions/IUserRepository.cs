using SettlersOfCrutan.Domain.Users;

namespace SettlersOfCrutan.Application.Abstractions;

public interface IUserRepository : IRepository<User, UserId>
{
    Task<User?> GetByPrincipalIdAsync(string principalId, CancellationToken ct = default);
}
