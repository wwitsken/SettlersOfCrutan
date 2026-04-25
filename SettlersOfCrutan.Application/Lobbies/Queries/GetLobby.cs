using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Lobbies.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Application.Lobbies.Queries;

public record GetLobbyQuery(LobbyId LobbyId) : IQuery<LobbyDto>;

public sealed class GetLobbyQueryHandler(ILobbyRepository lobbyRepository, ICurrentUser currentUser) : IQueryHandler<GetLobbyQuery, LobbyDto>
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<LobbyDto>> Handle(GetLobbyQuery query, CancellationToken ct = default)
    {
        var lobby = await _lobbyRepository.GetAsync(query.LobbyId, ct);

        if (lobby is null) return Result.Failure<LobbyDto>(DomainError.NotFound);

        var viewerId = await _currentUser.UserId();
        var dto = LobbyDto.FromLobby(lobby, viewerId);

        return Result.Success(dto);
    }
}
