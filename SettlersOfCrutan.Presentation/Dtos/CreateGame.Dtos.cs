using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Presentation.Dtos;

public record StartGameFromLobbyRequest(GameType GameType, string? GameName = "Crutan Game");