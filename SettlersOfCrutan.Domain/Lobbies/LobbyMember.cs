namespace SettlersOfCrutan.Domain.Lobbies;
public record LobbyMember(string UserId, bool IsHost, bool IsReady, string GameName = "Player");