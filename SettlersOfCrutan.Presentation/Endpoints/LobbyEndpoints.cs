using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Contracts.Lobbies;
using SettlersOfCrutan.Application.Lobbies.Commands;
using SettlersOfCrutan.Application.Lobbies.Queries;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Presentation.Auth;
using SettlersOfCrutan.Presentation.Extensions;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class LobbyEndpoints
{
    public static IEndpointRouteBuilder MapLobbyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/lobby").WithTags("Lobbies");

        group.MapPost("/create", async Task<Results<Ok<Guid>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            ICommandHandler<CreateLobbyCommand, Guid> handler,
            IUserProvider userProvider,
            CancellationToken ct) =>
        {
            Result<Guid> result = await handler.Handle(new CreateLobbyCommand(userProvider.GetUserId()), ct);

            return result.ToHttpResult();
        });

        group.MapGet("/{lobbyId:guid}", async Task<Results<Ok<LobbyDto>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid lobbyId,
            IQueryHandler<GetLobbyQuery, LobbyDto> handler,
            IUserProvider userProvider,
            CancellationToken ct) =>
        {
            var query = new GetLobbyQuery(lobbyId, userProvider.GetUserId());
            var result = await handler.Handle(query, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/{lobbyId:guid}/join", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid lobbyId,
            ICommandHandler<JoinLobbyCommand> handler,
            IUserProvider userProvider,
            CancellationToken ct) =>
        {
            var cmd = new JoinLobbyCommand(lobbyId, userProvider.GetUserId());
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/{lobbyId:guid}/leave", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid lobbyId,
            ICommandHandler<LeaveLobbyCommand> handler,
            IUserProvider userProvider,
            CancellationToken ct) =>
        {
            var cmd = new LeaveLobbyCommand(lobbyId, userProvider.GetUserId());
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/{lobbyId:guid}/ready", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid lobbyId,
            ICommandHandler<ChangeReadyStatusCommand> handler,
            IUserProvider userProvider,
            CancellationToken ct) =>
        {
            var cmd = new ChangeReadyStatusCommand(lobbyId, userProvider.GetUserId(), true);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/{lobbyId:guid}/unready", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid lobbyId,
            ICommandHandler<ChangeReadyStatusCommand> handler,
            IUserProvider userProvider,
            CancellationToken ct) =>
        {
            var cmd = new ChangeReadyStatusCommand(lobbyId, userProvider.GetUserId(), false);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        return app;
    }
}
