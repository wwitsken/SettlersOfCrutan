using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies.DomainEvents;
using SettlersOfCrutan.Domain.Users;
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
    public bool IsHost(UserId userId) => _members.Any(m => m.UserId == userId && m.IsHost);

    [JsonConstructor]
    private Lobby(LobbyId id, IReadOnlyList<LobbyMember> members, bool isOpen, DateTimeOffset dateCreated, int capacity)
    {
        Id = id;
        _members = [.. members];
        IsOpen = isOpen;
        DateCreated = dateCreated;
        Capacity = capacity;
    }

    public static Lobby Create(UserId hostUserId)
    {
        var lobby = new Lobby(
            new LobbyId { Value = Guid.NewGuid() },
            [],
            true,
            DateTimeOffset.UtcNow,
            capacity: 4);

        var host = LobbyMember.CreateHost(hostUserId, DateTime.Now);
        lobby._members.Add(host);
        lobby.AddDomainEvent(new LobbyMemberAddedDomainEvent(lobby.Id, host.Id));
        lobby.AddDomainEvent(new LobbyHostChangedDomainEvent(lobby.Id, host.Id));
        return lobby;
    }

    public Result<Nothing> AddMember(UserId userId)
    {
        if (!IsOpen) return Result<Nothing>.Failure(new("LobbyClosed", "Lobby is closed."));
        if (_members.Any(m => m.UserId == userId)) return Result<Nothing>.Failure(new("AlreadyJoinedLobby", "Player is already in the lobby."));
        if (_members.Count >= Capacity) return Result<Nothing>.Failure(new("LobbyFull", "Lobby is full."));

        var newMember = LobbyMember.CreateRegular(userId, DateTime.Now);
        _members.Add(newMember);
        AddDomainEvent(new LobbyMemberAddedDomainEvent(Id, newMember.Id));
        return Result.Success();
    }

    public Result<Nothing> RemoveMember(UserId userId)
    {
        var idx = _members.FindIndex(m => m.UserId == userId);
        if (idx < 0)
            return Result<Nothing>.Failure(new("NotInLobby", "Player is not in the lobby."));

        var removed = _members[idx];
        _members.RemoveAt(idx);

        AddDomainEvent(new LobbyMemberRemovedDomainEvent(Id, removed.Id));

        if (removed.IsHost && _members.Count > 0)
        {
            // simple policy: next joiner becomes host
            var nextPlayer = _members[0];
            if (nextPlayer is not null)
            {
                nextPlayer.SetHost(true);
                AddDomainEvent(new LobbyHostChangedDomainEvent(Id, nextPlayer.Id!));
            }
        }

        return Result.Success();
    }

    public Result<Nothing> SetReady(UserId userId, bool ready)
    {
        var idx = _members.FindIndex(m => m.UserId == userId);
        if (idx < 0)
            return Result<Nothing>.Failure(new("NotInLobby", "Player is not in the lobby."));

        var m = _members[idx];
        if (m.IsReady == ready) return Result.Success();

        m.SetReady(ready);
        AddDomainEvent(new LobbyMemberReadyStatusChangedDomainEvent(Id, m.Id, ready));
        return Result.Success();
    }

    public Result<Nothing> CanStartGame(UserId userId)
    {
        var idx = _members.FindIndex(m => m.UserId == userId);
        if (idx < 0)
            return Result.Failure(new("NotInLobby", "Player is not in the lobby."));
        var m = _members[idx];
        if (!m.IsHost)
            return Result.Failure(new("NotHost", "Only the host can start the game."));
        if (!AllReady())
            return Result.Failure(new("NotAllReady", "Not all players are ready."));

        return Result.Success();
    }

    private bool AllReady() => _members.Count > 0 && _members.All(m => m.IsReady);
}