using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Presentation.Extensions;

public static class StrongIdExtensions
{
    public static PlayerId ToPlayerId(string id) => new() { Value = id };
}
