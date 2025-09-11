using Microsoft.Extensions.DependencyInjection;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.Policies;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Generation;

namespace SettlersOfCrutan.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Concrete singletons
        services.AddSingleton<IDateTimeProvider, SystemClock>();

        // App services / policies
        services.AddScoped<IBoardGenerator, RandomBoardGenerator>();
        services.AddSingleton<StandardPriceCalculator>();

        // Scan and register command handlers, query handlers, and domain event handlers
        services.Scan(scan => scan
            .FromApplicationDependencies(a => a.GetName().Name!.StartsWith("SettlersOfCrutan"))
                // Command handlers (no result)
                .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<>)))
                    .AsSelfWithInterfaces()
                    .WithScopedLifetime()
                // Command handlers (with result)
                .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)))
                    .AsSelfWithInterfaces()
                    .WithScopedLifetime()
                // Query handlers
                .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)))
                    .AsSelfWithInterfaces()
                    .WithScopedLifetime()
                // Domain event handlers
                .AddClasses(c => c.AssignableTo(typeof(IDomainEventHandler<>)))
                    .AsSelfWithInterfaces()
                    .WithScopedLifetime());

        return services;
    }
}
