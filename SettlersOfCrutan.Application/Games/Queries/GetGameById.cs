using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Application.Games.Queries;

public record GetGameByIdQuery(Guid Id, string UserId) : IQuery<PlayerGameProjectionDto>;

public class GetGameByIdQueryHandler : IQueryHandler<GetGameByIdQuery, PlayerGameProjectionDto>
{
    private readonly IGameRepository _repository;

    public GetGameByIdQueryHandler(IGameRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PlayerGameProjectionDto>> Handle(GetGameByIdQuery query, CancellationToken ct = default)
    {
        Game? game = await _repository.GetAsync(new() { Value = query.Id }, ct);
        return game is not null
            ? Result.Success(PlayerGameProjectionDto.FromGame(game, query.UserId))
            : Result<PlayerGameProjectionDto>.Failure(DomainError.NotFound);
    }
}
