using System.Diagnostics.CodeAnalysis;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using System.Text.Json.Serialization;

namespace SettlersOfCrutan.Domain.Users;

public record UserId : BaseId<Guid>
{
    public static UserId Create(Guid id) => new() { Value = id };
}
public class User : AggregateRoot<UserId>
{
    public override UserId Id { get; init; } // surrogate key (internal to app)
    public required string PrincipalId { get; set; }
    public PlayerColor PreferredColor { get; private set; } = PlayerColor.White;
    public string Name { get; private set; } = "";

    [JsonConstructor]
    [SetsRequiredMembers]
    private User(UserId id, string principalId, PlayerColor preferredColor, string name)
    {
        Id = id;
        PrincipalId = principalId;
        PreferredColor = preferredColor;
        Name = name;
    }

    /// <summary>Creates a new persisted user row keyed by <paramref name="principalId"/> (e.g. OIDC sub / NameIdentifier).</summary>
    public static User RegisterNew(string principalId, string name = "")
    {
        var id = new UserId { Value = Guid.NewGuid() };
        return new User(id, principalId, PlayerColor.White, name);
    }

    public Result<Nothing> UpdateName(string name)
    {
        Name = name;
        return Result.Success();
    }

    public Result<Nothing> UpdatePreferredColor(PlayerColor color)
    {
        PreferredColor = color;
        return Result.Success();
    }
}