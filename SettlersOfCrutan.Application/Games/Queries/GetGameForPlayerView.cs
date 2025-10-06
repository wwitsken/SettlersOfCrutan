using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Games.Queries;

public record GetGameForPlayer(GameId GameId, string PlayerId) : IQuery<PlayerGameProjectionDto>;

public class GetGameForPlayerHandler(IGameRepository repository) : IQueryHandler<GetGameForPlayer, PlayerGameProjectionDto>
{
    private readonly IGameRepository _repository = repository;

    public async Task<Result<PlayerGameProjectionDto>> Handle(GetGameForPlayer query, CancellationToken ct = default)
    {
        Game? game = await _repository.GetAsync(query.GameId, ct);
        if (game is null) return Result<PlayerGameProjectionDto>.Failure(DomainError.NotFound);
        if (game.Players.Any(p => p.Id.Value == query.PlayerId) == false)
            return Result<PlayerGameProjectionDto>.Failure(DomainError.NotFound);
        return Result.Success(PlayerGameProjectionDto.FromGame(game, query.PlayerId));
    }
}