using Microsoft.Extensions.DependencyInjection;
using SettlersOfCrutan.Application.Games.Commands.Build;
using SettlersOfCrutan.Application.Games.Commands.Lifecycle;
using SettlersOfCrutan.Application.Games.Commands.TurnFlow;
using SettlersOfCrutan.Application.Games.Queries;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.PriceCalculators;
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
        services.AddSingleton<StandardPriceCalculator>();

        //services.AddScoped<CreateTodoListCommandHandler>();
        //services.AddScoped<GetTodoListByIdQueryHandler>();

        services.AddScoped<GetGameByIdQueryHandler>();
        services.AddScoped<CreateGameCommandHandler>();
        services.AddScoped<JoinGameCommandHandler>();
        services.AddScoped<BuildInitialCommandHandler>();
        services.AddScoped<RollDiceCommandHandler>();


        // Scan and register all domain event handlers as Scoped
        services.Scan(scan => scan
            .FromApplicationDependencies(a => a.GetName().Name!.StartsWith("SettlersOfCrutan"))
            .AddClasses(c => c.AssignableTo(typeof(IDomainEventHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
