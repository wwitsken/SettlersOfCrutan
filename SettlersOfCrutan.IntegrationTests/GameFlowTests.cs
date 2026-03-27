using SettlersOfCrutan.Application.Games.DTOs;
using SettlersOfCrutan.Domain.Games;
using SettlersOfCrutan.Presentation.Dtos;

namespace SettlersOfCrutan.IntegrationTests;

[Collection("AppHost collection")]
public class GameFlowTests(AppHostFixture fixture)
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);
    private readonly AppHostFixture _fixture = fixture;

    /*

    [Fact]
    public async Task CreateGame_ThenJoinPlayers_ThenEndTurn_Flow_Works()
    {
        var ctoken = new CancellationTokenSource(DefaultTimeout).Token;
        var api = _fixture.ApiClient;

        // 1) Create game
        StartGameFromLobbyRequest createReq = new("123",  GameType.BaseGame);

        var createResp = await api.PostAsJsonAsync("/games/create", createReq, ctoken);
        createResp.EnsureSuccessStatusCode();
        CreateGameResultDto? createResult = await createResp.Content.ReadFromJsonAsync<CreateGameResultDto>(cancellationToken: ctoken);
        Assert.NotNull(createResult);
        Assert.Equal(3, createResult!.PlayerOrder.Count);

        // Location header contains new game URI
        var location = createResp.Headers.Location?.ToString();
        Assert.False(string.IsNullOrWhiteSpace(location));
        var idStr = location!.Split('/').Last();
        Assert.True(Guid.TryParse(idStr, out var gameId));

        // 2) Join players
        async Task<HttpResponseMessage> JoinAsync(string userId)
        {
            var join = new { PlayerId = userId };
            return await api.PostAsJsonAsync($"/games/{gameId}/play/join", join, ctoken);
        }

        var join1 = await JoinAsync("p1");
        Assert.True(join1.IsSuccessStatusCode);

        var join2 = await JoinAsync("p2");
        Assert.True(join2.IsSuccessStatusCode);

        var badJoin = await JoinAsync("p4");
        Assert.False(badJoin.IsSuccessStatusCode);

        var join3 = await JoinAsync("p3");
        Assert.True(join2.IsSuccessStatusCode);

        var getResp = await api.GetAsync($"/games/{gameId}", ctoken);
        Assert.True(getResp.IsSuccessStatusCode);
    }
    */

    /*
    [Fact]
    public async Task CreateExistingGame()
    {
        var ctoken = new CancellationTokenSource(DefaultTimeout).Token;

        var api = _fixture.ApiClient;
    }
    */
}
