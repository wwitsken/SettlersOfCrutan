using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies;
using SettlersOfCrutan.Domain.PlayerPresence.DomainEvents;
using System.Text.Json.Serialization;

namespace SettlersOfCrutan.Domain.PlayerPresence;

public sealed record PlayerPresenceId : BaseId<Guid>;
public sealed record ConnectionId : BaseId<string>;

public enum PresenceState
{
    Offline = 0,
    Online = 1,
    InLobby = 2,
    InGame = 3
}


public sealed class PlayerPresence : AggregateRoot<PlayerPresenceId>
{
    private readonly HashSet<string> _activeConnectionIds = new(StringComparer.Ordinal);

    public override PlayerPresenceId Id { get; init; }
    public PlayerId PlayerId { get; }
    public IReadOnlyCollection<string> ActiveConnectionIds => _activeConnectionIds;
    public LobbyId? LobbyId { get; private set; }
    public GameId? GameId { get; private set; }
    public DateTime LastSeenUtc { get; private set; }

    // Policy: clear Lobby/Game when the last connection disappears
    public bool ClearMembershipOnZeroConnections { get; }

    public PresenceState State =>
        _activeConnectionIds.Count == 0 ? PresenceState.Offline :
        GameId is not null ? PresenceState.InGame :
        LobbyId is not null ? PresenceState.InLobby :
        PresenceState.Online;

    [JsonConstructor]
    private PlayerPresence(
        PlayerPresenceId id,
        PlayerId playerId,
        bool clearMembershipOnZeroConnections,
        DateTime nowUtc)
    {
        Id = id;
        PlayerId = playerId;
        ClearMembershipOnZeroConnections = clearMembershipOnZeroConnections;
        LastSeenUtc = nowUtc;
    }

    public static PlayerPresence CreateNew(PlayerId playerId, bool clearMembershipOnZeroConnections, DateTime nowUtc)
        => new(new() { Value = Guid.NewGuid() }, playerId, clearMembershipOnZeroConnections, nowUtc);

    // --- Behavior ---

    public void Heartbeat(DateTime nowUtc)
    {
        LastSeenUtc = nowUtc;
    }

    public void Connect(ConnectionId connectionId, DateTime nowUtc)
    {
        var wasOffline = _activeConnectionIds.Count == 0;
        if (_activeConnectionIds.Add(connectionId.Value))
        {
            LastSeenUtc = nowUtc;
            AddDomainEvent(new PlayerConnected(PlayerId, connectionId, nowUtc));
            if (wasOffline) AddDomainEvent(new PlayerPresenceBecameOnline(PlayerId, nowUtc));
        }
    }

    public void Disconnect(ConnectionId connectionId, DateTime nowUtc)
    {
        if (_activeConnectionIds.Remove(connectionId.Value))
        {
            LastSeenUtc = nowUtc;
            AddDomainEvent(new PlayerDisconnected(PlayerId, connectionId, nowUtc));

            if (_activeConnectionIds.Count == 0)
            {
                AddDomainEvent(new PlayerPresenceBecameOffline(PlayerId, nowUtc));

                if (ClearMembershipOnZeroConnections)
                {
                    ClearMembershipInternal(nowUtc, cause: "ZeroConnections");
                }
            }
        }
    }

    public void TimeoutIfStale(TimeSpan timeout, DateTime nowUtc)
    {
        if (_activeConnectionIds.Count == 0) return; // already offline

        if (nowUtc - LastSeenUtc > timeout)
        {
            _activeConnectionIds.Clear();
            AddDomainEvent(new PlayerPresenceTimedOut(PlayerId, LastSeenUtc, nowUtc));
            AddDomainEvent(new PlayerPresenceBecameOffline(PlayerId, nowUtc));

            if (ClearMembershipOnZeroConnections)
            {
                ClearMembershipInternal(nowUtc, cause: "Timeout");
            }
        }
    }

    public void JoinLobby(LobbyId lobbyId, DateTime nowUtc)
    {
        RequireOnline();

        if (GameId is not null)
            throw new InvalidOperationException("Cannot join a lobby while in a game. Leave game or promote via PromoteToGame.");

        if (LobbyId == lobbyId) return;

        // If already in another lobby, leave it first
        if (LobbyId is LobbyId current)
        {
            LobbyId = null;
            AddDomainEvent(new PlayerLeftLobby(PlayerId, current, nowUtc));
        }

        LobbyId = lobbyId;
        LastSeenUtc = nowUtc;
        AddDomainEvent(new PlayerJoinedLobby(PlayerId, lobbyId, nowUtc));
    }

    public void LeaveLobby(DateTime nowUtc)
    {
        if (LobbyId is LobbyId current)
        {
            LobbyId = null;
            LastSeenUtc = nowUtc;
            AddDomainEvent(new PlayerLeftLobby(PlayerId, current, nowUtc));
        }
    }

    public void JoinGame(GameId gameId, DateTime nowUtc)
    {
        RequireOnline();

        // Optional policy: leaving lobby automatically when joining game.
        if (LobbyId is LobbyId currentLobby)
        {
            LobbyId = null;
            AddDomainEvent(new PlayerLeftLobby(PlayerId, currentLobby, nowUtc));
        }

        if (GameId == gameId) return;

        if (GameId is not null && GameId != gameId)
            throw new InvalidOperationException("Already in a different game. Leave first.");

        GameId = gameId;
        LastSeenUtc = nowUtc;
        AddDomainEvent(new PlayerJoinedGame(PlayerId, gameId, nowUtc));
    }

    public void LeaveGame(DateTime nowUtc)
    {
        if (GameId is GameId current)
        {
            GameId = null;
            LastSeenUtc = nowUtc;
            AddDomainEvent(new PlayerLeftGame(PlayerId, current, nowUtc));
        }
    }

    public void PromoteToGame(LobbyId fromLobby, GameId toGame, DateTime nowUtc)
    {
        RequireOnline();

        if (LobbyId != fromLobby)
            throw new InvalidOperationException("Player is not in the specified lobby.");

        LobbyId = null;
        GameId = toGame;
        LastSeenUtc = nowUtc;

        AddDomainEvent(new PlayerPromotedFromLobbyToGame(PlayerId, fromLobby, toGame, nowUtc));
        AddDomainEvent(new PlayerLeftLobby(PlayerId, fromLobby, nowUtc));
        AddDomainEvent(new PlayerJoinedGame(PlayerId, toGame, nowUtc));
    }

    private void ClearMembershipInternal(DateTime nowUtc, string cause)
    {
        if (GameId is GameId g)
        {
            GameId = null;
            AddDomainEvent(new PlayerLeftGame(PlayerId, g, nowUtc));
        }
        if (LobbyId is LobbyId l)
        {
            LobbyId = null;
            AddDomainEvent(new PlayerLeftLobby(PlayerId, l, nowUtc));
        }
    }

    private void RequireOnline()
    {
        if (_activeConnectionIds.Count == 0)
            throw new InvalidOperationException("Player is offline. At least one active connection is required.");
    }
}