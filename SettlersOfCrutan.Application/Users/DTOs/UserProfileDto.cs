using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Users.DTOs;

public record UserProfileDto
{
    public required Guid UserId { get; init; }
    public required string DisplayName { get; init; }
    public required PlayerColor PreferredColor { get; init; }
}
