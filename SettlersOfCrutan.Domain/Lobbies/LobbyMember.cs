using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Users;
using System.Text.Json.Serialization;

namespace SettlersOfCrutan.Domain.Lobbies;

public record LobbyMemberId() : BaseId<Guid>;
public class LobbyMember : Entity<LobbyMemberId>
{
    [JsonConstructor]
    public LobbyMember(LobbyMemberId id, UserId userId, bool isHost, bool isReady, DateTimeOffset? joined)
    {
        Id = id;
        UserId = userId;
        IsHost = isHost;
        IsReady = isReady;
        Joined = joined;
    }

    public override LobbyMemberId Id { get; init; }
    public UserId UserId { get; private set; }
    public bool IsHost { get; private set; }
    public bool IsReady { get; private set; }
    public DateTimeOffset? Joined { get; private set; }
    public void SetReady(bool ready) => IsReady = ready;
    public void SetHost(bool isHost) => IsHost = isHost;
    public static LobbyMember CreateHost(UserId userId, DateTimeOffset joined) =>
        new(new() { Value = Guid.NewGuid() }, userId, true, false, joined);
    public static LobbyMember CreateRegular(UserId userId, DateTimeOffset joined) =>
        new(new() { Value = Guid.NewGuid() }, userId, false, false, joined);
}
