using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Lobbies.DomainEvents;
using System.Text.Json.Serialization;

namespace SettlersOfCrutan.Domain.Lobbies;

public record LobbyId : BaseId<Guid>;
public class Lobby : AggregateRoot<LobbyId>
{
    public override LobbyId Id { get; init; }

    private readonly List<LobbyMember> _lobbyMembers = [];
    public IReadOnlyList<LobbyMember> LobbyMembers => [.. _lobbyMembers];

    public DateTimeOffset DateCreated { get; init; }

    public bool IsOpen { get; private set; } = true;

    [JsonConstructor]
    private Lobby(LobbyId id, List<LobbyMember> lobbyMembers, bool isOpen, DateTimeOffset dateCreated)
    {
        Id = id;
        _lobbyMembers = lobbyMembers;
        IsOpen = isOpen;
        DateCreated = dateCreated;
    }
    public static Lobby Create(string hostUserId)
    {
        var lobby = new Lobby(
            new LobbyId() { Value = Guid.NewGuid() },
            [new(hostUserId, true, false)],
            true,
            DateTimeOffset.Now);
        return lobby;
    }
    public Result<Nothing> AddMember(string userId)
    {
        if (!IsOpen) return Result<Nothing>.Failure(new Error("LobbyClosed", "Lobby is closed."));
        if (_lobbyMembers.Any(m => m.UserId == userId)) return Result<Nothing>.Failure(new Error("AlreadyJoinedLobby", "User is already in the lobby."));
        if (_lobbyMembers.Count >= 4) return Result<Nothing>.Failure(new Error("LobbyFull", "Lobby is full."));
        var member = new LobbyMember(userId, false, false);
        _lobbyMembers.Add(member);
        AddDomainEvent(new LobbyMemberAddedDomainEvent(Id, member));
        return Result.Success();
    }
    public Result<Nothing> RemoveMember(string userId)
    {
        var member = _lobbyMembers.FirstOrDefault(m => m.UserId == userId);
        if (member is null) return Result<Nothing>.Failure(new Error("NotInLobby", "User is not in the lobby."));
        _lobbyMembers.Remove(member);
        AddDomainEvent(new LobbyMemberRemovedDomainEvent(Id, member));
        // If the host leaves, assign a new host if possible
        if (member.IsHost && _lobbyMembers.Count > 0)
        {
            var newHost = _lobbyMembers[0];
            _lobbyMembers.RemoveAt(0);
            _lobbyMembers.Add(new LobbyMember(newHost.UserId, true, newHost.IsReady));
            AddDomainEvent(new LobbyHostChangedDomainEvent(Id, newHost.UserId));
        }
        return Result.Success();
    }
    public Result<Nothing> SetMemberReady(string userId, bool ready)
    {
        var index = _lobbyMembers.FindIndex(m => m.UserId == userId);
        if (index == -1) return Result<Nothing>.Failure(new Error("NotInLobby", "User is not in the lobby."));
        var member = _lobbyMembers[index];
        _lobbyMembers[index] = member with { IsReady = ready };
        AddDomainEvent(new LobbyMemberReadyStatusChangedDomainEvent(Id, userId, ready));
        return Result.Success();
    }
    public void CloseLobby() => IsOpen = false;
    public void OpenLobby() => IsOpen = true;
}