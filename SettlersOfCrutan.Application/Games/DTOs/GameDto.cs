using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Games.DTOs;

public record GameDto
{
    public required PublicGameDto Game { get; set; }
    public required PrivateGameDto MyPrivateGameInfo { get; set; }
    public static GameDto FromGame(Game game, string userId)
        => new()
        {
            Game = game.ToDto(),
            MyPrivateGameInfo = game.ToPrivateDto(userId)
        };

    public static Dictionary<string, GameDto> UserViewsFromGame(Game game)
    {
        return game.Players
            .Select(p => p.UserId)
            .OfType<string>()
            .ToDictionary(u => u, u => FromGame(game, u));
    }
}
