using SettlersOfCrutan.Domain.Users;

namespace SettlersOfCrutan.Application.Abstractions;

public interface ICurrentUser
{
    public Task<UserId> UserId();
}
