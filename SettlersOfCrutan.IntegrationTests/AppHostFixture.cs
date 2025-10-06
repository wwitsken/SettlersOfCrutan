using Aspire.Hosting;

namespace SettlersOfCrutan.IntegrationTests;
public class AppHostFixture : IAsyncLifetime
{
    private DistributedApplication _app = default!;
    public HttpClient ApiClient => _app.CreateHttpClient("api");

    public async Task InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.SettlersOfCrutan_AppHost>();

        _app = await appHost.BuildAsync();
        await _app.StartAsync();
    }

    public async Task DisposeAsync() => await _app.DisposeAsync();
}