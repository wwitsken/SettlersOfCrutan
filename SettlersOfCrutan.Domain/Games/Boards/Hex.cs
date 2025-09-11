using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;
using System.Text.Json.Serialization;

namespace SettlersOfCrutan.Domain.Games.Boards;

public record HexId : BaseId<HexCoord>;

public class Hex(HexCoord coordinate)
    : Entity<HexId>
{
    [JsonIgnore]
    public override HexId Id { get; init; } = new() { Value = coordinate };
    public HexCoord Coordinate { get; init; } = coordinate;
    public ResourceCardType Resource { get; set; }
    public int? NumberToken { get; set; }
    public bool HasRobber { get; set; }
}
