using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Games.Commands.Lifecycle;
using SettlersOfCrutan.Application.Games.Queries;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Presentation.Dtos;
using SettlersOfCrutan.Presentation.Extensions;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class BaseGameEndpoints
{
    public static IEndpointRouteBuilder MapGameViewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games").WithTags("Game:Lifecycle");

        group.MapPost("/create", async Task<IResult> (
            [FromBody] CreateGameRequest command,
            CreateGameCommandHandler handler,
            CancellationToken ct) =>
                {
                    var cmd = new CreateGameCommand(command.GameName, [.. command.UserIds], GameType.BaseGame);
                    var result = await handler.Handle(cmd, ct);
                    return result.IsSuccess
                        ? TypedResults.Created($"/games/{result.Value.Value}")
                        : result.Error.ToHttpResult();
                });

        group.MapGet("/{id:guid}", async Task<IResult> (
            Guid id,
            [FromServices] GetGameByIdQueryHandler handler,
            CancellationToken ct) =>
        {
            var query = new GetGameByIdQuery(new GameId { Value = id });
            var result = await handler.Handle(query, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToHttpResult();
        });

        return app;
    }
};