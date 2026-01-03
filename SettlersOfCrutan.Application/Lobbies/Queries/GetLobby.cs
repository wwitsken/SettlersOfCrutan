using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Lobbies.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Application.Lobbies.Queries;
public record GetLobbyQuery(Guid LobbyId, string UserViewerId) : IQuery<LobbyDto>;

public sealed class GetLobbyQueryHandler(ILobbyRepository lobbyRepository) : IQueryHandler<GetLobbyQuery, LobbyDto>
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;

    public async Task<Result<LobbyDto>> Handle(GetLobbyQuery query, CancellationToken ct = default)
    {
        var lobby = await _lobbyRepository.GetAsync(new LobbyId() { Value = query.LobbyId }, ct);

        if (lobby is null) return Result.Failure<LobbyDto>(DomainError.NotFound);

        var dto = new LobbyDto()
        {
            LobbyId = lobby.Id.Value,
            LobbyMembers = [.. lobby.Members.Select(p => new LobbyMemberDto
            {
                Id = p.Id.ToString(),
                DisplayName = p.DisplayName ?? "",
                IsHost = p.IsHost,
                IsReady = p.IsReady,
                IsMe = p.UserId == query.UserViewerId
            })]
        };

        return Result.Success(dto);
    }
}