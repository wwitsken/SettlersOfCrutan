using Microsoft.Extensions.DependencyInjection;
using SettlersOfCrutan.Application.Games;
using SettlersOfCrutan.Application.Todos;

namespace SettlersOfCrutan.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<CreateTodoListCommandHandler>();
        services.AddScoped<GetTodoListByIdQueryHandler>();
        services.AddScoped<GenerateBoardCommandHandler>();
        services.AddScoped<GetGameByIdQueryHandler>();

        return services;
    }
}
