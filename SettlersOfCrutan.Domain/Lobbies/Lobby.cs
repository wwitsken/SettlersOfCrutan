using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies.DomainEvents;
using System.Text.Json.Serialization;

namespace SettlersOfCrutan.Domain.Lobbies;

public record LobbyId : BaseId<Guid>;
public class Lobby : AggregateRoot<LobbyId>
{
    public override LobbyId Id { get; init; }
    public DateTimeOffset DateCreated { get; init; }
    public bool IsOpen { get; private set; } = true;
    public int Capacity { get; private set; } = 4;

    private readonly Dictionary<PlayerId, MemberState> _members = [];
    public IReadOnlyCollection<MemberState> Members => [.. _members.Values];
    public bool IsHost(PlayerId pid) => _members.TryGetValue(pid, out var m) && m.IsHost;

    [JsonConstructor]
    private Lobby(LobbyId id, Dictionary<PlayerId, MemberState> members, bool isOpen, DateTimeOffset dateCreated, int capacity)
    {
        Id = id;
        _members = members ?? [];
        IsOpen = isOpen;
        DateCreated = dateCreated;
        Capacity = capacity;
    }

    public static Lobby Create(PlayerId host)
    {
        var lobby = new Lobby(
            new LobbyId { Value = Guid.NewGuid() },
            [],
            true,
            DateTimeOffset.UtcNow,
            capacity: 4);

        lobby._members.Add(host, MemberState.Host());
        lobby.AddDomainEvent(new LobbyMemberAddedDomainEvent(lobby.Id, host));
        lobby.AddDomainEvent(new LobbyHostChangedDomainEvent(lobby.Id, host));
        return lobby;
    }

    /// <summary>
    /// Admission rule: the application layer should already have verified the player's Presence:
    /// - player is Online, and
    /// - player is not in another lobby or is joining this one.
    /// </summary>
    public Result<Nothing> AddMember(PlayerId playerId)
    {
        if (!IsOpen) return Result<Nothing>.Failure(new("LobbyClosed", "Lobby is closed."));
        if (_members.ContainsKey(playerId)) return Result<Nothing>.Failure(new("AlreadyJoinedLobby", "Player is already in the lobby."));
        if (_members.Count >= Capacity) return Result<Nothing>.Failure(new("LobbyFull", "Lobby is full."));

        _members[playerId] = MemberState.Regular();
        AddDomainEvent(new LobbyMemberAddedDomainEvent(Id, playerId));
        return Result.Success();
    }

    public Result<Nothing> RemoveMember(PlayerId playerId)
    {
        if (!_members.Remove(playerId, out var removed))
            return Result<Nothing>.Failure(new("NotInLobby", "Player is not in the lobby."));

        AddDomainEvent(new LobbyMemberRemovedDomainEvent(Id, playerId));

        if (removed.IsHost && _members.Count > 0)
        {
            // simple policy: next joiner becomes host
            var next = _members.Keys.First();
            _members[next] = _members[next] with { IsHost = true };
            AddDomainEvent(new LobbyHostChangedDomainEvent(Id, next));
        }

        return Result.Success();
    }

    public Result<Nothing> SetReady(PlayerId playerId, bool ready)
    {
        if (!_members.TryGetValue(playerId, out var m))
            return Result<Nothing>.Failure(new("NotInLobby", "Player is not in the lobby."));
        if (m.IsReady == ready) return Result.Success();

        _members[playerId] = m with { IsReady = ready };
        AddDomainEvent(new LobbyMemberReadyStatusChangedDomainEvent(Id, playerId, ready));
        return Result.Success();
    }

    public void CloseLobby() => IsOpen = false;
    public void OpenLobby() => IsOpen = true;

    public bool AllReady() => _members.Count > 0 && _members.Values.All(m => m.IsReady);
}

// Lobby-local state only
public readonly record struct MemberState(bool IsHost, bool IsReady, string? DisplayName = null)
{
    public static MemberState Host(string? displayName = default) => new(true, false, displayName);
    public static MemberState Regular(string? displayName = default) => new(false, false, displayName);
}