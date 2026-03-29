namespace SettlersOfCrutan.Presentation.Dtos;

public record ResolveRobberRequest(HexCoordDto NewRobberHex, string? VictimPlayerId = null);
