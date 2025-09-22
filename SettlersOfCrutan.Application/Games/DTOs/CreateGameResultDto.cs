namespace SettlersOfCrutan.Application.Games.DTOs;
public record CreateGameResultDto(Guid GameId, Dictionary<int, string> PlayerOrder);
