using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Users.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Users;

namespace SettlersOfCrutan.Application.Users.Queries;


public sealed record GetUserProfileById(UserId UserId) : IQuery<UserProfileDto>;

public sealed class GetUserProfileByIdHandler(
    IUserRepository userRepository) : IQueryHandler<GetUserProfileById, UserProfileDto>
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<Result<UserProfileDto>> Handle(GetUserProfileById query, CancellationToken ct = default)
    {
        var user = await _userRepository.GetAsync(query.UserId, ct);
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
