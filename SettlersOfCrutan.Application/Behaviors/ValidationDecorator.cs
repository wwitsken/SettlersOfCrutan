using FluentValidation;
using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Application.Validation;
using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Application.Behaviors;
internal static class ValidationDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> inner,
        IEnumerable<IValidator<TCommand>> validators)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken ct = default)
        {
            var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(command, ct)));
            var failures = results.SelectMany(r => r.Errors).ToArray();

            if (failures.Length > 0)
            {
                var errors = failures
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                return ValidationFailure<TResponse>.FromErrors(errors);
            }

            return await inner.Handle(command, ct);
        }
    }

    internal sealed class CommandHandler<TCommand>(
    ICommandHandler<TCommand> inner,
    IEnumerable<IValidator<TCommand>> validators)
    : ICommandHandler<TCommand>
    where TCommand : ICommand
    {
        public async Task<Result<Nothing>> Handle(TCommand command, CancellationToken ct = default)
        {
            var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(command, ct)));
            var failures = results.SelectMany(r => r.Errors).ToArray();

            if (failures.Length > 0)
            {
                var errors = failures
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                return ValidationFailure<Nothing>.FromErrors(errors);
            }

            return await inner.Handle(command, ct);
        }
    }
}
