using SettlersOfCrutan.Presentation.Dtos;

namespace SettlersOfCrutan.IntegrationTests;

[Collection("AppHost collection")]
public class GameFlowTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);
    private readonly AppHostFixture _fixture;
    public GameFlowTests(AppHostFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task CreateGame_ThenJoinPlayers_ThenEndTurn_Flow_Works()
    {
        var ctoken = new CancellationTokenSource(DefaultTimeout).Token;

        await using var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.SettlersOfCrutan_AppHost>(ctoken);
        appHost.Services.ConfigureHttpClientDefaults(builder => builder.AddStandardResilienceHandler());

        await using var app = await appHost.BuildAsync(ctoken).WaitAsync(DefaultTimeout, ctoken);
        await app.StartAsync(ctoken).WaitAsync(DefaultTimeout, ctoken);

        // API client
        var api = app.CreateHttpClient("api");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("api", ctoken).WaitAsync(DefaultTimeout, ctoken);

        // 1) Create game
        CreateGameRequest createReq = new("Test Game", ["p1", "p2", "p3"], "BaseGame");

        var createResp = await api.PostAsJsonAsync("/games/create", createReq, ctoken);

        createResp.EnsureSuccessStatusCode();

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

        // 3) End first turn (setup phase continues)
        var endTurnPayload = new { PlayerId = "p1" };
        var endTurnResp = await api.PostAsJsonAsync($"/games/{gameId}/turn/end", endTurnPayload, ctoken);
        Assert.True(endTurnResp.IsSuccessStatusCode);

        // 4) Get game and verify state exists
        var getResp = await api.GetAsync($"/games/{gameId}", ctoken);
        Assert.True(getResp.IsSuccessStatusCode);
    }
}
