using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Games.Queries;

public record GetGameForPlayer(GameId GameId) : IQuery<GameDto>;

public class GetGameForPlayerHandler(IGameRepository repository, ICurrentUser currentUser) : IQueryHandler<GetGameForPlayer, GameDto>
{
    private readonly IGameRepository _repository = repository;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<GameDto>> Handle(GetGameForPlayer query, CancellationToken ct = default)
    {
        Game? game = await _repository.GetAsync(query.GameId, ct);
        if (game is null) return Result<GameDto>.Failure(DomainError.NotFound);

        var userId = await _currentUser.UserId();
        if (game.Players.Any(p => p.UserId == userId) == false)
            return Result<GameDto>.Failure(DomainError.NotFound);

        return Result.Success(GameDto.FromGame(game, userId));
    }
}
