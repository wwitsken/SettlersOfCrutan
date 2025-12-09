using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using System.Text.Json.Serialization;

namespace SettlersOfCrutan.Domain.Lobbies;

public record LobbyMemberId() : BaseId<Guid>;
public class LobbyMember : Entity<LobbyMemberId>
{
    [JsonConstructor]
    public LobbyMember(LobbyMemberId id, PlayerId? playerId, bool isHost, bool isReady, string? displayName, DateTimeOffset? joined)
    {
        Id = id;
        PlayerId = playerId;
        IsHost = isHost;
        IsReady = isReady;
        DisplayName = displayName;
        Joined = joined;
    }

    public override LobbyMemberId Id { get; init; }
    public PlayerId? PlayerId { get; private set; }
    public bool IsHost { get; private set; }
    public bool IsReady { get; private set; }
    public string? DisplayName { get; private set; }
    public DateTimeOffset? Joined { get; private set; }
    public void SetReady(bool ready) => IsReady = ready;
    public void SetDisplayName(string displayName) => DisplayName = displayName;
    public void SetHost(bool isHost) => IsHost = isHost;
    public static LobbyMember CreateHost(PlayerId playerId, DateTimeOffset joined, string? displayName = default) =>
        new(new() { Value = Guid.NewGuid() }, playerId, true, false, displayName, joined);
    public static LobbyMember CreateRegular() =>
        new(new() { Value = Guid.NewGuid() }, default, false, false, default, default);
    public static LobbyMember CreateRegular(PlayerId playerId, DateTimeOffset joined, string? displayName = default) =>
        new(new() { Value = Guid.NewGuid() }, playerId, false, false, displayName, joined);
}
