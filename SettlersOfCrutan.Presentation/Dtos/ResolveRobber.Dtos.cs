namespace SettlersOfCrutan.Presentation.Dtos;

public record ResolveRobberRequest(string PlayerId, string VictimPlayerId, HexCoordDto NewRobberHex);
