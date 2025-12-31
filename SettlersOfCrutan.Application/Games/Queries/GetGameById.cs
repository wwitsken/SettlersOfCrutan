using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Games.Queries;

public record GetGameByIdQuery(GameId Id) : IQuery<GameDto>;

public class GetGameByIdQueryHandler(IGameRepository repository) : IQueryHandler<GetGameByIdQuery, GameDto>
{
    private readonly IGameRepository _repository = repository;

    public async Task<Result<GameDto>> Handle(GetGameByIdQuery query, CancellationToken ct = default)
    {
        Game? game = await _repository.GetAsync(query.Id, ct);
        return game is not null
            ? Result<GameDto>.Success(game.ToDto())
            : Result<GameDto>.Failure(DomainError.InvalidOperation);
    }
}
