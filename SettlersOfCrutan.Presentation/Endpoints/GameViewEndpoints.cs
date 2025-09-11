using Microsoft.AspNetCore.Http.HttpResults;
using SettlersOfCrutan.Application.Games.Queries;
using SettlersOfCrutan.Domain.Games;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class GameViewEndpoints
{
    public static IEndpointRouteBuilder MapGameViewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games/{id:guid}").WithTags("Games");

        group.MapGet("/", async Task<Results<Ok<Game>, NotFound, ValidationProblem>> (
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

        return app;
    }
};