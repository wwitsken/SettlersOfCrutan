using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies;

namespace SettlersOfCrutan.Application.Abstractions;
public interface IRealtimePublisher
{
    Task ToLobbyAsync(LobbyId lobbyId, string channel, object payload, CancellationToken ct = default);
    Task ToGameAsync(GameId gameId, string channel, object payload, CancellationToken ct = default);
    Task ToUserAsync(string userId, string channel, object payload, CancellationToken ct = default);
}