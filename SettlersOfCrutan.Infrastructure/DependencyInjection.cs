using Microsoft.Extensions.DependencyInjection;
using SettlersOfCrutan.Infrastructure.Redis;
using SettlersOfCrutan.Infrastructure.Redis.Repositories;
using StackExchange.Redis;
using SettlersOfCrutan.Application.Todos;

namespace SettlersOfCrutan.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services)
    {
        // Expose IDatabase (Aspire registered IConnectionMultiplexer)
        services.AddScoped(sp =>
        {
            var mux = sp.GetRequiredService<IConnectionMultiplexer>();
            return mux.GetDatabase();
        });

        // Repositories
        services.AddScoped(typeof(RedisRepository<,>));
        services.AddScoped<ITodoListRepository, RedisTodoListRepository>();

        return services;
    }
}