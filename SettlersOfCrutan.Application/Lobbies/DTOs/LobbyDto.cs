using SettlersOfCrutan.Domain.Lobbies;
using SettlersOfCrutan.Domain.Users;

namespace SettlersOfCrutan.Application.Lobbies.DTOs;

public record LobbyDto
{
    public Guid LobbyId { get; set; }
    public List<LobbyMemberDto> LobbyMembers { get; set; } = [];

    public static LobbyDto FromLobby(Lobby lobby)
    {
        return new LobbyDto
        {
            LobbyId = lobby.Id.Value,
            LobbyMembers = [.. lobby.Members.Select(p => new LobbyMemberDto
            {
                Id = p.Id.ToString(),
                UserId = p.UserId.Value,
                IsHost = p.IsHost,
                IsReady = p.IsReady
            })]
        };
    }

    public static LobbyDto FromLobby(Lobby lobby, UserId viewerUserId)
    {
        return new LobbyDto
        {
            LobbyId = lobby.Id.Value,
            LobbyMembers = [.. lobby.Members.Select(p => new LobbyMemberDto
            {
                Id = p.Id.ToString(),
                UserId = p.UserId.Value,
                IsHost = p.IsHost,
                IsReady = p.IsReady,
                IsMe = p.UserId == viewerUserId
            })]
        };
    }

    public static Dictionary<UserId, LobbyDto> UserViewsFromLobby(Lobby lobby)
    {
        return lobby.Members
            .Select(m => m.UserId)
            .Distinct()
            .ToDictionary(u => u, u => FromLobby(lobby, u));
    }
}

public record LobbyMemberDto
{
    public required string Id { get; set; }
    public required Guid UserId { get; set; }
    public bool IsHost { get; set; }
    public bool IsReady { get; set; }
    public bool IsMe { get; set; }
}
