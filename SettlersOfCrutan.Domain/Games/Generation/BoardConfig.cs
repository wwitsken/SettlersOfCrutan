using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;

namespace SettlersOfCrutan.Domain.Games.Generation;

/// <summary>
/// A port sits on the edge between a board hex (<see cref="LandHex"/>) and an adjacent
/// off-board / sea hex (<see cref="SeaHex"/>).
/// </summary>
public readonly record struct PortEdgeSlot(HexCoord LandHex, HexCoord SeaHex);

public sealed record BoardConfig
(
    int Radius,
    IReadOnlyDictionary<ResourceCardType, int> ResourceCounts,
    IReadOnlyDictionary<PortType, int> Ports,
    IReadOnlyList<int> NumberTokens,
    IReadOnlyList<PortEdgeSlot>? FixedPortEdges = null
);

public static class FixedPortEdgePlacement
{
    public static void ValidateOrThrow(BoardConfig config, HashSet<HexCoord> boardHexes)
    {
        var slots = config.FixedPortEdges;
        if (slots is null || slots.Count == 0)
            return;

        var totalPorts = config.Ports.Values.Sum();
        if (slots.Count != totalPorts)
        {
            throw new ArgumentException(
                $"FixedPortEdges count {slots.Count} must equal total ports {totalPorts}.");
        }

        var seen = new HashSet<Edge>();
        foreach (var slot in slots)
        {
            if (!boardHexes.Contains(slot.LandHex))
            {
                throw new ArgumentException(
                    $"Fixed port land hex {slot.LandHex} is not on the board for radius {config.Radius}.");
            }

            if (boardHexes.Contains(slot.SeaHex))
            {
                throw new ArgumentException(
                    $"Fixed port sea hex {slot.SeaHex} must be off the board.");
            }

            if (!slot.LandHex.GetAdjacentHexCoords().Values.Contains(slot.SeaHex))
            {
                throw new ArgumentException(
                    $"Land hex {slot.LandHex} is not adjacent to sea hex {slot.SeaHex}.");
            }

            var edge = new Edge(slot.LandHex, slot.SeaHex).Normalize();
            if (!seen.Add(edge))
                throw new ArgumentException("Duplicate fixed port edge.");
        }
    }
}
