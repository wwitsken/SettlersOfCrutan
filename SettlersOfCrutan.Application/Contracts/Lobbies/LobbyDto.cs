namespace SettlersOfCrutan.Application.Contracts.Lobbies;
public record LobbyDto
{
    public Guid LobbyId { get; set; }
    public List<LobbyPlayerDto> LobbyPlayers { get; set; }
}

public record LobbyPlayerDto
{
    public string GameName { get; set; }
    public bool IsMe { get; set; }
    public bool IsHost { get; set; }
    public bool IsReady { get; set; }
}