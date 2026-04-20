using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Presentation.Dtos;

public sealed record UpdateUserProfileRequest(string? DisplayName, PlayerColor? PreferredColor);
public sealed record GetUserProfilesRequest(ICollection<Guid> UserIds);
