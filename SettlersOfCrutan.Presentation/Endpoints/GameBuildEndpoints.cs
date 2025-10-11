using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.Commands.Build;
using SettlersOfCrutan.Domain.Games.Resources;
using SettlersOfCrutan.Presentation.Auth;
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
            IUserProvider userProvider,
            ICommandHandler<BuildRoadCommand> handler,
            CancellationToken ct) =>
        {
            var cmd = new BuildRoadCommand(id, userProvider.GetUserId(), request.EdgeCoordinate);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/settlement", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            [FromBody] BuildSettlementRequest request,
            IUserProvider userProvider,
            ICommandHandler<BuildSettlementCommand> handler,
            CancellationToken ct) =>
        {
            var cmd = new BuildSettlementCommand(id, userProvider.GetUserId(), request.VertexCoordinate);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();

        });

        group.MapPost("/city", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            [FromBody] UpgradeSettlementToCityRequest request,
            IUserProvider userProvider,
            ICommandHandler<UpgradeSettlementToCityCommand> handler,
            CancellationToken ct) =>
        {
            var cmd = new UpgradeSettlementToCityCommand(id, userProvider.GetUserId(), request.VertexCoordinate);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/development-card", async Task<Results<Ok<DevelopmentCardType>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid id,
            IUserProvider userProvider,
            ICommandHandler<BuyDevelopmentCardCommand, DevelopmentCardType> handler,
            CancellationToken ct) =>
        {
            var cmd = new BuyDevelopmentCardCommand(id, userProvider.GetUserId());
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        return app;
    }
}
