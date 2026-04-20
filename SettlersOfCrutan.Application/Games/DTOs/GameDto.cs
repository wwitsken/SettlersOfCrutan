using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Users;

namespace SettlersOfCrutan.Application.Games.DTOs;

public record GameDto
{
    public required PublicGameDto Game { get; set; }
    public required PrivateGameDto MyPrivateGameInfo { get; set; }
    public static GameDto FromGame(Game game, UserId userId)
        => new()
        {
            Game = game.ToDto(),
            MyPrivateGameInfo = game.ToPrivateDto(userId)
        };

    public static Dictionary<UserId, GameDto> UserViewsFromGame(Game game)
    {
        return game.Players
            .Select(p => p.UserId)
            .ToDictionary(u => u, u => FromGame(game, u));
    }
}
