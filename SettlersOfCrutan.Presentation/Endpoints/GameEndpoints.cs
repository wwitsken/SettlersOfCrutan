using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Games.Commands.Lifecycle;
using SettlersOfCrutan.Application.Games.Queries;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Presentation.Dtos;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class GameEndpoints
{
    public static IEndpointRouteBuilder MapGameEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games")
                       .WithTags("Games");

        //group.MapPost("/generate", async Task<Results<Created, ValidationProblem>> (
        //    [FromBody] GenerateBoardCommand command,
        //    GenerateBoardCommandHandler handler,
        //    CancellationToken ct) =>
        //{
        //    Result<GameId> result = await handler.Handle(command, ct);
        //    return result.IsSuccess
        //    ? TypedResults.Created($"/games/{result.Value.Value}")
        //    : TypedResults.ValidationProblem(new Dictionary<string, string[]> { [result.Error.Code] = [result.Error.Message] });
        //});

        group.MapPost("/", async Task<Results<Created, ValidationProblem>> (
            [FromBody] CreateGameCommand command,
            CreateGameCommandHandler handler,
            CancellationToken ct) =>
        {
            var cmd = new CreateGameCommand(command.GameName, command.UserIds, GameType.BaseGame);
            var result = await handler.Handle(cmd, ct);
            return result.IsSuccess
                ? TypedResults.Created($"/games/{result.Value.Value}")
                : TypedResults.ValidationProblem(new Dictionary<string, string[]> { [result.Error.Code] = [result.Error.Message] });
        });

        group.MapGet("/{id:guid}", async Task<Results<Ok<Game>, NotFound, ValidationProblem>> (
            Guid id,
            GetGameByIdQueryHandler handler,
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

        group.MapPost("/{id:guid}/join", async Task<Results<Ok, NotFound, ValidationProblem>> (
            Guid id,
            [FromBody] JoinGameDto request,
            JoinGameCommandHandler handler,
            CancellationToken ct) =>
        {
            PlayerId playerId = new() { Value = request.PlayerId };
            GameId gameId = new() { Value = id };
            var cmd = new JoinGameCommand(gameId, playerId);

            var result = await handler.Handle(cmd, ct);
            if (result.IsSuccess)
                return TypedResults.Ok();
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
}