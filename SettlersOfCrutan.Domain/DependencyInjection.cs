using Microsoft.Extensions.DependencyInjection;
using SettlersOfCrutan.Domain.Generation;

namespace SettlersOfCrutan.Domain;
public static class DependencyInjection
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IBoardGenerator, RandomBoardGenerator>();
        return services;
    }
}
