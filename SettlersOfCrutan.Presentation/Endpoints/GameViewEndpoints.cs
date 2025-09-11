using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Games.Commands.Lifecycle;
using SettlersOfCrutan.Application.Games.Queries;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Presentation.Dtos;
using SettlersOfCrutan.Presentation.Extensions;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class GameViewEndpoints
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

        group.MapGet("/{id:guid}", async Task<Results<Ok<Game>, NotFound, ValidationProblem>> (
            Guid id,
            [FromServices] GetGameByIdQueryHandler handler,
            CancellationToken ct) =>
        {
            var query = new GetGameByIdQuery(new GameId { Value = id });
            var result = await handler.Handle(query, ct);

            if (result.IsSuccess && result.Value is not null)
                return TypedResults.Ok(result.Value);

            if (result.IsFailure && result.Error.Code == "Validation")
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    [result.Error.Code] = [result.Error.Message]
                });
            }

            return TypedResults.NotFound();
        });

        return app;
    }
};