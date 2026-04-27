using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Games.Policies;
using SettlersOfCrutan.Domain.Games.Generation;

namespace SettlersOfCrutan.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // App services / policies
        services.AddScoped<IBoardGenerator, RandomBoardGenerator>();
        services.AddSingleton<StandardPriceCalculator>();

        // Scan and register command handlers, query handlers

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

        /* Registration order matters - behaviors will be called in order, BOTTOM to TOP: */
        /* 3RD */        services.Decorate(typeof(ICommandHandler<>), typeof(Behaviors.GameNotificationDecorator.CommandHandler<>));
        /* 2ND */        services.Decorate(typeof(ICommandHandler<>), typeof(Behaviors.GameWinEvaluatorDecorator.CommandHandler<>));
        /* 1ST */        services.Decorate(typeof(ICommandHandler<>), typeof(Behaviors.ValidationDecorator.CommandHandler<>));

        /* Registration order matters - behaviors will be called in order, BOTTOM to TOP: */
        /* 3RD */        services.Decorate(typeof(ICommandHandler<,>), typeof(Behaviors.GameNotificationDecorator.CommandHandler<,>));
        /* 2ND */        services.Decorate(typeof(ICommandHandler<,>), typeof(Behaviors.GameWinEvaluatorDecorator.CommandHandler<,>));
        /* 1ST */        services.Decorate(typeof(ICommandHandler<,>), typeof(Behaviors.ValidationDecorator.CommandHandler<,>));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        return services;
    }
}
