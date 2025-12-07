using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Contracts.Lobbies;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Application.Lobbies.Queries;
public record GetLobbyQuery(Guid LobbyId, string UserId) : IQuery<LobbyDto>;

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
            LobbyPlayers = [.. lobby.Members.Select(p => new LobbyPlayerDto
            {
                GameName = p.DisplayName ?? "",
                IsMe = p.PlayerId == query.UserId,
                IsHost = p.IsHost,
                IsReady = p.IsReady
            })]
        };

        return Result.Success(dto);
    }
}