using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.Policies;
using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.Games.Generation;

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

        // Switch to Implemented Interfaces to allow for validators to get injected
        services.Scan(scan => scan
            .FromApplicationDependencies(a => a.GetName().Name!.StartsWith("SettlersOfCrutan"))
                .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                .AddClasses(c => c.AssignableTo(typeof(IDomainEventHandler<>)))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());

        services.Decorate(typeof(ICommandHandler<>), typeof(Behaviors.ValidationDecorator.CommandHandler<>));
        services.Decorate(typeof(ICommandHandler<,>), typeof(Behaviors.ValidationDecorator.CommandHandler<,>));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        return services;
    }
}
