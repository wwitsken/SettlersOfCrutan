using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Games;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class GameEndpoints
{
    public static IEndpointRouteBuilder MapGameEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games")
                       .WithTags("Games");

        group.MapPost("/generate", async Task<Results<Created, ValidationProblem>> (
            [FromBody] GenerateBoardCommand command,
            GenerateBoardCommandHandler handler,
            CancellationToken ct) =>
        {
            var cmd = new GenerateBoardCommand(command.UserIds);
            Result<GameId> result = await handler.Handle(cmd, ct);
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
                    [result.Error.Code] = new[] { result.Error.Message }
                });
            }

            return TypedResults.NotFound();
        });

        return app;
    }
}