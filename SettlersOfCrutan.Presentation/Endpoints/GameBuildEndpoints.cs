using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.Commands.Build;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Games.Boards.Coordinates;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Presentation.Dtos;
using SettlersOfCrutan.Presentation.Extensions;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class GameBuildEndpoints
{
    public static IEndpointRouteBuilder MapGameBuildEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games/{id:guid}/build").WithTags("Game:Build");

        group.MapPost("/road", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            [FromBody] BuildRoadRequest request,
            ICommandHandler<BuildRoadCommand> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            Edge edge = request.EdgeCoordinate.ToDomain();
            var cmd = new BuildRoadCommand(gameId, edge);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/settlement", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            [FromBody] BuildSettlementRequest request,
            ICommandHandler<BuildSettlementCommand> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            Vertex vertex = request.VertexCoordinate.ToDomain();
            var cmd = new BuildSettlementCommand(gameId, vertex);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/city", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            [FromBody] UpgradeSettlementToCityRequest request,
            ICommandHandler<UpgradeSettlementToCityCommand> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            Vertex vertex = request.VertexCoordinate.ToDomain();
            var cmd = new UpgradeSettlementToCityCommand(gameId, vertex);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/development-card", async Task<Results<Ok<DevelopmentCardType>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            ICommandHandler<BuyDevelopmentCardCommand, DevelopmentCardType> handler,
            CancellationToken ct) =>
        {
            GameId gameId = new() { Value = id };
            var cmd = new BuyDevelopmentCardCommand(gameId);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.RequireAuthorization();

        return app;
    }
}
