using SettlersOfCrutan.Domain.Lobbies;

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
                DisplayName = p.DisplayName ?? "",
                IsHost = p.IsHost,
                IsReady = p.IsReady
            })]
        };
    }
}

public record LobbyMemberDto
{
    public required string Id { get; set; }
    public string DisplayName { get; set; } = "";
    public bool IsHost { get; set; }
    public bool IsReady { get; set; }
}