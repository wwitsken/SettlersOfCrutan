using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Users.DTOs;
using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Application.Users.Queries;

public sealed record GetCurrentUserProfile : IQuery<UserProfileDto>;

public sealed class GetCurrentUserProfileHandler(
    ICurrentUser currentUser,
    IUserRepository userRepository) : IQueryHandler<GetCurrentUserProfile, UserProfileDto>
{
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<Result<UserProfileDto>> Handle(GetCurrentUserProfile query, CancellationToken ct = default)
    {
        var userId = await _currentUser.UserId();
        var user = await _userRepository.GetAsync(userId, ct);
        if (user is null)
            return Result<UserProfileDto>.Failure(DomainError.NotFound);

        return Result.Success(new UserProfileDto
        {
            UserId = user.Id.Value,
            DisplayName = user.Name,
            PreferredColor = user.PreferredColor,
        });
    }
}
