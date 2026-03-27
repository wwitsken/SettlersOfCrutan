using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Lobbies;
using SettlersOfCrutan.Domain.Lobbies.DomainEvents;

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

    public static LobbyDto FromLobby(Lobby lobby, string userId)
    {
        return new LobbyDto
        {
            LobbyId = lobby.Id.Value,
            LobbyMembers = [.. lobby.Members.Select(p => new LobbyMemberDto
            {
                Id = p.Id.ToString(),
                DisplayName = p.DisplayName ?? "",
                IsHost = p.IsHost,
                IsReady = p.IsReady,
                IsMe = p.UserId == userId
            })]
        };
    }

    public static Dictionary<string, LobbyDto> UserViewsFromLobby(Lobby lobby)
    {
        return lobby.Members
            .Select(m => m.UserId)
            .OfType<string>()
            .ToDictionary(u => u, u => FromLobby(lobby, u));
    }
}

public record LobbyMemberDto
{
    public required string Id { get; set; }
    public string DisplayName { get; set; } = "";
    public bool IsHost { get; set; }
    public bool IsReady { get; set; }
    public bool IsMe { get; set; }
}