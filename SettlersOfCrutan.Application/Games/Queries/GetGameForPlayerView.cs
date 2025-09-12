using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;

namespace SettlersOfCrutan.Application.Games.Queries;

public record GetGameForPlayerView(GameId Id) : IQuery<Game>;

public class GetGameForPlayerViewHandler : IQueryHandler<GetGameForPlayerView, Game>
{
    private readonly IGameRepository _repository;

    public GetGameForPlayerViewHandler(IGameRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Game>> Handle(GetGameForPlayerView query, CancellationToken ct = default)
    {
        Game? game = await _repository.GetAsync(query.Id, ct);
        return game is not null
            ? Result<Game>.Success(game)
            : Result<Game>.Failure(DomainError.InvalidOperation);
    }
}

public sealed record PlayerGameView
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<Hex> Hexes { get; set; }
    public List<Road> Roads { get; set; }
    public List<PopulationCenter> PopulationCenters { get; set; }
}

public sealed record Settlement
{
    public Vertex Vertex { get; set; }
    public string Player { get; set; }
}