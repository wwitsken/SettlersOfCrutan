using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Users.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Users;

namespace SettlersOfCrutan.Application.Users.Queries;


public sealed record GetUserProfilesByIds(IEnumerable<UserId> UserIds) : IQuery<IEnumerable<UserProfileDto>>;

public sealed class GetUserProfilesByIdsHandler(
    IUserRepository userRepository) : IQueryHandler<GetUserProfilesByIds, IEnumerable<UserProfileDto>>
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<Result<IEnumerable<UserProfileDto>>> Handle(GetUserProfilesByIds query, CancellationToken ct = default)
    {
        var users = await _userRepository.GetManyAsync(query.UserIds, ct);

        return Result.Success(users.Select(u => new UserProfileDto
        {
            UserId = u.Id.Value,
            DisplayName = u.Name,
            PreferredColor = u.PreferredColor,
        }));
    }
}
