using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Coordinates;
using System.Text.Json.Serialization;

namespace SettlersOfCrutan.Domain.Games;

public record HexId : BaseId<HexCoord>;

public class Hex(HexCoord coordinate)
    : Entity<HexId>
{
    [JsonIgnore]
    public override HexId Id { get; init; } = new() { Value = coordinate };
    public ResourceType Resource { get; set; }
    public int? NumberToken { get; set; }
    public bool HasRobber { get; set; }
    public HexCoord Coordinate { get; init; } = coordinate;
}
