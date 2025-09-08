using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Games.Queries;

public record GetGameByIdQuery(GameId Id) : IQuery<Game>;

public class GetGameByIdQueryHandler : IQueryHandler<GetGameByIdQuery, Game>
{
    private readonly IGameRepository _repository;

    public GetGameByIdQueryHandler(IGameRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Game>> Handle(GetGameByIdQuery query, CancellationToken ct = default)
    {
        Game? game = await _repository.GetAsync(query.Id, ct);
        return game is not null
            ? Result<Game>.Success(game)
            : Result<Game>.Failure(DomainError.InvalidOperation);
    }
}
