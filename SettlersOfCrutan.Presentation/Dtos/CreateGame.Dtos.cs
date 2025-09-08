using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Presentation.Dtos;

public record CreateGameRequest(string GameName, List<string> UserIds, GameType GameType);