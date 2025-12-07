using Microsoft.Extensions.DependencyInjection;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Infrastructure.Outbox;
using SettlersOfCrutan.Infrastructure.Redis;
using SettlersOfCrutan.Infrastructure.Redis.Repositories;
using SettlersOfCrutan.Infrastructure.SignalR;
using StackExchange.Redis;

namespace SettlersOfCrutan.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Expose IDatabase (Aspire registered IConnectionMultiplexer)
        services.AddScoped(sp =>
        {
            var mux = sp.GetRequiredService<IConnectionMultiplexer>();
            return mux.GetDatabase();
        });

        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        // Repositories
        services.AddScoped(typeof(RedisRepository<,>));
        //services.AddScoped<ITodoListRepository, RedisTodoListRepository>();
        services.AddScoped<IGameRepository, RedisGameRepository>();
        services.AddScoped<ILobbyRepository, RedisLobbyRepository>();
        //services.AddScoped<IPlayerPresenceRepository, RedisPlayerPresenceRepository>();

        // SignalR
        services.AddScoped<IRealtimePublisher, SignalRRealtimePublisher>();

        return services;
    }
}