namespace SettlersOfCrutan.Presentation.Dtos;

public record CreateGameRequest(string GameName, List<string> UserIds, string GameType);