using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies;
using SettlersOfCrutan.Domain.PlayerPresence.DomainEvents;
using System.Text.Json.Serialization;

namespace SettlersOfCrutan.Domain.PlayerPresence;

public sealed record PlayerPresenceId : BaseId<string>;
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

    /// <summary>
    /// PlayerPresenceID is the PlayerID = the User
    /// </summary>
    public override PlayerPresenceId Id { get; init; }
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
        bool clearMembershipOnZeroConnections,
        DateTime nowUtc)
    {
        Id = id;
        ClearMembershipOnZeroConnections = clearMembershipOnZeroConnections;
        LastSeenUtc = nowUtc;
    }

    public static PlayerPresence CreateNew(PlayerId playerId, bool clearMembershipOnZeroConnections, DateTime nowUtc)
        => new(new() { Value = playerId.Value }, clearMembershipOnZeroConnections, nowUtc);

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
            AddDomainEvent(new PlayerConnected(new() { Value = Id.Value }, connectionId, nowUtc));
            if (wasOffline) AddDomainEvent(new PlayerPresenceBecameOnline(new() { Value = Id.Value }, nowUtc));
        }
    }

    public void Disconnect(ConnectionId connectionId, DateTime nowUtc)
    {
        if (_activeConnectionIds.Remove(connectionId.Value))
        {
            LastSeenUtc = nowUtc;
            AddDomainEvent(new PlayerDisconnected(new() { Value = Id.Value }, connectionId, nowUtc));

            if (_activeConnectionIds.Count == 0)
            {
                AddDomainEvent(new PlayerPresenceBecameOffline(new() { Value = Id.Value }, nowUtc));

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
            AddDomainEvent(new PlayerPresenceTimedOut(new() { Value = Id.Value }, LastSeenUtc, nowUtc));
            AddDomainEvent(new PlayerPresenceBecameOffline(new() { Value = Id.Value }, nowUtc));

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
            AddDomainEvent(new PlayerLeftLobby(new() { Value = Id.Value }, current, nowUtc));
        }

        LobbyId = lobbyId;
        LastSeenUtc = nowUtc;
        AddDomainEvent(new PlayerJoinedLobby(new() { Value = Id.Value }, lobbyId, nowUtc));
    }

    public void LeaveLobby(DateTime nowUtc)
    {
        if (LobbyId is LobbyId current)
        {
            LobbyId = null;
            LastSeenUtc = nowUtc;
            AddDomainEvent(new PlayerLeftLobby(new() { Value = Id.Value }, current, nowUtc));
        }
    }

    public void JoinGame(GameId gameId, DateTime nowUtc)
    {
        RequireOnline();

        // Optional policy: leaving lobby automatically when joining game.
        if (LobbyId is LobbyId currentLobby)
        {
            LobbyId = null;
            AddDomainEvent(new PlayerLeftLobby(new() { Value = Id.Value }, currentLobby, nowUtc));
        }

        if (GameId == gameId) return;

        if (GameId is not null && GameId != gameId)
            throw new InvalidOperationException("Already in a different game. Leave first.");

        GameId = gameId;
        LastSeenUtc = nowUtc;
        AddDomainEvent(new PlayerJoinedGame(new() { Value = Id.Value }, gameId, nowUtc));
    }

    public void LeaveGame(DateTime nowUtc)
    {
        if (GameId is GameId current)
        {
            GameId = null;
            LastSeenUtc = nowUtc;
            AddDomainEvent(new PlayerLeftGame(new() { Value = Id.Value }, current, nowUtc));
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

        AddDomainEvent(new PlayerPromotedFromLobbyToGame(new() { Value = Id.Value }, fromLobby, toGame, nowUtc));
        AddDomainEvent(new PlayerLeftLobby(new() { Value = Id.Value }, fromLobby, nowUtc));
        AddDomainEvent(new PlayerJoinedGame(new() { Value = Id.Value }, toGame, nowUtc));
    }

    private void ClearMembershipInternal(DateTime nowUtc, string cause)
    {
        if (GameId is GameId g)
        {
            GameId = null;
            AddDomainEvent(new PlayerLeftGame(new() { Value = Id.Value }, g, nowUtc));
        }
        if (LobbyId is LobbyId l)
        {
            LobbyId = null;
            AddDomainEvent(new PlayerLeftLobby(new() { Value = Id.Value }, l, nowUtc));
        }
    }

    private void RequireOnline()
    {
        if (_activeConnectionIds.Count == 0)
            throw new InvalidOperationException("Player is offline. At least one active connection is required.");
    }
}