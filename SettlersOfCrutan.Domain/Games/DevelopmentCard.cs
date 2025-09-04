namespace SettlersOfCrutan.Domain.Games;

public enum DevelopmentCardType
{
    Knight,         // Enable the robber and steal a resource card from another player on Build/Trade phase (sends to Steal state)
    VictoryPoint,   // Keep secret until the end of the game, worth 1 victory point
    RoadBuilding,   // Place 2 roads as if you had built them (no resource cost)
    YearOfPlenty,   // Take any 2 resource cards from the bank
    Monopoly        // Announce 1 type of resource. All other players must give you all their resource cards of that type
}

public record DevelopmentCardAmount(DevelopmentCardType Type, int Quantity);
