using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Games.Queries;

public record GetGameByIdQuery(GameId Id) : IQuery<PublicGameDto>;

public class GetGameByIdQueryHandler(IGameRepository repository) : IQueryHandler<GetGameByIdQuery, PublicGameDto>
{
    private readonly IGameRepository _repository = repository;

    public async Task<Result<PublicGameDto>> Handle(GetGameByIdQuery query, CancellationToken ct = default)
    {
        Game? game = await _repository.GetAsync(query.Id, ct);
        return game is not null
            ? Result<PublicGameDto>.Success(game.ToDto())
            : Result<PublicGameDto>.Failure(DomainError.InvalidOperation);
    }
}
