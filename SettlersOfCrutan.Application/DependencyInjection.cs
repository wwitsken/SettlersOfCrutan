using Microsoft.Extensions.DependencyInjection;
using SettlersOfCrutan.Application.Games;
using SettlersOfCrutan.Application.Todos;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Generation;

namespace SettlersOfCrutan.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, SystemClock>();
        services.AddScoped<IBoardGenerator, RandomBoardGenerator>();
        services.AddScoped<CreateTodoListCommandHandler>();
        services.AddScoped<GetTodoListByIdQueryHandler>();
        services.AddScoped<GenerateBoardCommandHandler>();
        services.AddScoped<GetGameByIdQueryHandler>();

        return services;
    }
}
