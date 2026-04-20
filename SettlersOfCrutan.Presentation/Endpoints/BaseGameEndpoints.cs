using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.Commands.Lifecycle;
using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Application.Games.Queries;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Presentation.Extensions;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class BaseGameEndpoints
{
    public static IEndpointRouteBuilder MapBaseGameEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games").WithTags("Game:Lifecycle");

        group.MapPost("/{id:guid}/join", async Task<Results<Ok<Guid>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            ICommandHandler<JoinGameCommand, GameId> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            var cmd = new JoinGameCommand(gameId);
            var result = await handler.Handle(cmd, ct);
            return result.UnwrapId<GameId, Guid>().ToHttpResult();
        }).RequireAuthorization();

        group.MapGet("/{id:guid}", async Task<Results<Ok<GameDto>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            [FromServices] IQueryHandler<GetGameForPlayer, GameDto> handler,
            CancellationToken ct) =>
        {
            var query = new GetGameForPlayer(new GameId { Value = id });
            Result<GameDto> result = await handler.Handle(query, ct);
            return result.ToHttpResult();
        }).RequireAuthorization();

        return app;
    }
}
