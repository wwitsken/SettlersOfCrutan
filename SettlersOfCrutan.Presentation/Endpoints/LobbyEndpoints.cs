using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Lobbies.Commands;
using SettlersOfCrutan.Application.Lobbies.DTOs;
using SettlersOfCrutan.Application.Lobbies.Queries;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Domain.Lobbies;
using SettlersOfCrutan.Presentation.Dtos;
using SettlersOfCrutan.Presentation.Extensions;

namespace SettlersOfCrutan.Presentation.Endpoints;

public static class LobbyEndpoints
{
    public static IEndpointRouteBuilder MapLobbyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/lobby").WithTags("Lobbies");

        group.MapPost("/create", async Task<Results<Ok<Guid>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            ICommandHandler<CreateLobbyCommand, Guid> handler,
            CancellationToken ct) =>
        {
            Result<Guid> result = await handler.Handle(new CreateLobbyCommand(), ct);

            return result.ToHttpResult();
        });

        group.MapGet("/{lobbyId:guid}", async Task<Results<Ok<LobbyDto>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid lobbyId,
            IQueryHandler<GetLobbyQuery, LobbyDto> handler,
            CancellationToken ct) =>
        {
            var query = new GetLobbyQuery(new LobbyId { Value = lobbyId });
            var result = await handler.Handle(query, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/{lobbyId:guid}/join", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid lobbyId,
            ICommandHandler<JoinLobbyCommand> handler,
            CancellationToken ct) =>
        {
            var cmd = new JoinLobbyCommand(new LobbyId { Value = lobbyId });
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/{lobbyId:guid}/leave", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid lobbyId,
            ICommandHandler<LeaveLobbyCommand> handler,
            CancellationToken ct) =>
        {
            var cmd = new LeaveLobbyCommand(new LobbyId { Value = lobbyId });
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/{lobbyId:guid}/ready", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid lobbyId,
            ICommandHandler<ChangeReadyStatusCommand> handler,
            CancellationToken ct) =>
        {
            var cmd = new ChangeReadyStatusCommand(new LobbyId { Value = lobbyId }, true);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/{lobbyId:guid}/unready", async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid lobbyId,
            ICommandHandler<ChangeReadyStatusCommand> handler,
            CancellationToken ct) =>
        {
            var cmd = new ChangeReadyStatusCommand(new LobbyId { Value = lobbyId }, false);
            var result = await handler.Handle(cmd, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/{lobbyId:guid}/start-game", async Task<Results<Ok<Guid>, NotFound, ValidationProblem, BadRequest<ProblemDetails>>> (
            Guid lobbyId,
            [FromBody] StartGameFromLobbyRequest req,
            ICommandHandler<StartGameFromLobbyCommand, GameId> handler,
            CancellationToken ct) =>
        {
            var cmd = new StartGameFromLobbyCommand(new LobbyId { Value = lobbyId }, req.GameType, req.GameName);
            Result<GameId> result = await handler.Handle(cmd, ct);
            return result.UnwrapId<GameId, Guid>().ToHttpResult();
        }).RequireAuthorization();

        group.RequireAuthorization();

        return app;
    }
}
