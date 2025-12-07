using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies.DomainEvents;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace SettlersOfCrutan.Domain.Lobbies;

public record LobbyId : BaseId<Guid>;

public class Lobby : AggregateRoot<LobbyId>
{
    public override LobbyId Id { get; init; }
    public DateTimeOffset DateCreated { get; init; }
    public bool IsOpen { get; private set; } = true;
    public int Capacity { get; private set; } = 4;

    private readonly List<LobbyMember> _members = [];
    public IReadOnlyList<LobbyMember> Members => [.. _members];
    public bool IsHost(PlayerId pid) => _members.Any(m => m.Id == pid && m.IsHost);

    [JsonConstructor]
    private Lobby(LobbyId id, IReadOnlyList<LobbyMember> members, bool isOpen, DateTimeOffset dateCreated, int capacity)
    {
        Id = id;
        _members = [.. members];
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

        lobby._members.Add(LobbyMember.CreateHost(host, DateTime.Now));
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
        if (_members.Any(m => m.PlayerId == playerId)) return Result<Nothing>.Failure(new("AlreadyJoinedLobby", "Player is already in the lobby."));
        if (_members.Count >= Capacity) return Result<Nothing>.Failure(new("LobbyFull", "Lobby is full."));

        _members.Add(LobbyMember.CreateRegular(playerId, DateTime.Now));
        AddDomainEvent(new LobbyMemberAddedDomainEvent(Id, playerId));
        return Result.Success();
    }

    public Result<Nothing> RemoveMember(PlayerId playerId)
    {
        var idx = _members.FindIndex(m => m.PlayerId == playerId);
        if (idx < 0)
            return Result<Nothing>.Failure(new("NotInLobby", "Player is not in the lobby."));

        var removed = _members[idx];
        _members.RemoveAt(idx);

        AddDomainEvent(new LobbyMemberRemovedDomainEvent(Id, playerId));

        if (removed.IsHost && _members.Count > 0)
        {
            // simple policy: next joiner becomes host
            var nextPlayer = _members[0];
            if (nextPlayer is not null)
            {
                nextPlayer.SetHost(true);
                AddDomainEvent(new LobbyHostChangedDomainEvent(Id, nextPlayer.PlayerId!));
            }
        }

        return Result.Success();
    }

    public Result<Nothing> SetReady(PlayerId playerId, bool ready)
    {
        var idx = _members.FindIndex(m => m.PlayerId == playerId);
        if (idx < 0)
            return Result<Nothing>.Failure(new("NotInLobby", "Player is not in the lobby."));

        var m = _members[idx];
        if (m.IsReady == ready) return Result.Success();

        m.SetReady(ready);
        AddDomainEvent(new LobbyMemberReadyStatusChangedDomainEvent(Id, playerId, ready));
        return Result.Success();
    }

    public void CloseLobby() => IsOpen = false;
    public void OpenLobby() => IsOpen = true;

    public bool AllReady() => _members.Count > 0 && _members.All(m => m.IsReady);
}

// Lobby-local state only
public record MemberState(PlayerId PlayerId, bool IsHost, bool IsReady, string? DisplayName = null)
{
    public static MemberState Host(PlayerId playerId, string? displayName = default) => new(playerId, true, false, displayName);
    public static MemberState Regular(PlayerId playerId, string? displayName = default) => new(playerId, false, false, displayName);
}

public class LobbyMember(LobbyMemberId lobbyMemberId, PlayerId? playerId, bool? isHost, bool? isReady, DateTimeOffset? joined, string? displayName = null) : Entity<LobbyMemberId>
{
    public override LobbyMemberId Id { get; init; } = lobbyMemberId;
    public PlayerId? PlayerId { get; private set; } = playerId;
    public bool IsHost { get; private set; } = isHost ?? false;
    public bool IsReady { get; private set; } = isReady ?? false;
    public string? DisplayName { get; private set; } = displayName;
    public DateTimeOffset? Joined { get; private set; } = joined;
    public void SetReady(bool ready) => IsReady = ready;
    public void SetDisplayName(string displayName) => DisplayName = displayName;
    public void SetHost(bool isHost) => IsHost = isHost;
    public static LobbyMember CreateHost(PlayerId playerId, DateTimeOffset joined, string? displayName = default) =>
        new(new() { Value = Guid.NewGuid() }, playerId, true, false, joined, displayName);
    public static LobbyMember CreateRegular() =>
        new(new() { Value = Guid.NewGuid() }, default, false, false, default, default);
    public static LobbyMember CreateRegular(PlayerId playerId, DateTimeOffset joined, string? displayName = default) =>
        new(new() { Value = Guid.NewGuid() }, playerId, false, false, joined, displayName);
}

public record LobbyMemberId() : BaseId<Guid>;