using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using SettlersOfCrutan.Application.Games;
using SettlersOfCrutan.Application.Todos;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Generation;

namespace SettlersOfCrutan.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Concrete singletons
        services.AddSingleton<IDateTimeProvider, SystemClock>();

        // App services
        services.AddScoped<IBoardGenerator, RandomBoardGenerator>();
        services.AddScoped<CreateTodoListCommandHandler>();
        services.AddScoped<GetTodoListByIdQueryHandler>();
        services.AddScoped<GenerateBoardCommandHandler>();
        services.AddScoped<GetGameByIdQueryHandler>();
        services.AddScoped<CreateGameCommandHandler>();

        // Scan and register all domain event handlers as Scoped
        services.Scan(scan => scan
            .FromApplicationDependencies(a => a.GetName().Name!.StartsWith("SettlersOfCrutan"))
            .AddClasses(c => c.AssignableTo(typeof(IDomainEventHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
