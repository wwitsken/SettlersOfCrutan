using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Application.Lobbies.DTOs;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies;
using SettlersOfCrutan.Domain.Users;

namespace SettlersOfCrutan.Application.Abstractions.Realtime;

public static class RealtimePublisherExtensions
{
    /// <summary>Loads each <see cref="User"/> by id and publishes the per-user payload (e.g. SignalR targets <see cref="User.PrincipalId"/>).</summary>
    public static async Task PublishGameStateToAllPlayersAsync(
        this IRealtimePublisher publisher,
        IUserRepository users,
        GameId gameId,
        IReadOnlyDictionary<UserId, GameDto> viewsByUserId,
        DateTimeOffset timestamp,
        string eventName,
        CancellationToken ct = default)
    {
        var tasks = viewsByUserId.Select(async kvp =>
        {
            var user = await users.GetAsync(kvp.Key, ct);
            if (user is null) return;
            await publisher.UpdateGameAsync(gameId, user, timestamp, eventName, kvp.Value, ct);
        });
        await Task.WhenAll(tasks);
    }

    public static async Task PublishLobbyStateToAllMembersAsync(
        this IRealtimePublisher publisher,
        IUserRepository users,
        LobbyId lobbyId,
        IReadOnlyDictionary<UserId, LobbyDto> viewsByUserId,
        DateTimeOffset timestamp,
        string eventName,
        CancellationToken ct = default)
    {
        var tasks = viewsByUserId.Select(async kvp =>
        {
            var user = await users.GetAsync(kvp.Key, ct);
            if (user is null) return;
            await publisher.UpdateLobbyAsync(lobbyId, user, timestamp, eventName, kvp.Value, ct);
        });
        await Task.WhenAll(tasks);
    }

    public static async Task<IReadOnlyList<User>> ResolveUsersForRealtimeAsync(
        this IUserRepository users,
        IEnumerable<UserId> userIds,
        CancellationToken ct = default)
    {
        var list = new List<User>();
        foreach (var id in userIds.Distinct())
        {
            var u = await users.GetAsync(id, ct);
            if (u is not null) list.Add(u);
        }

        return list;
    }
}
