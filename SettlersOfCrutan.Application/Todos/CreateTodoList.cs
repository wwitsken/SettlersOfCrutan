using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Todos;

namespace SettlersOfCrutan.Application.Todos;
public record CreateTodoListCommand(string Title);

public class CreateTodoListCommandHandler
{
    private readonly ITodoListRepository _repository;

    public CreateTodoListCommandHandler(ITodoListRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TodoListId>> Handle(CreateTodoListCommand command, CancellationToken ct = default)
    {
        var newId = new TodoListId() { Value = Guid.NewGuid() };
        var todoList = new TodoList(newId, command.Title, []);
        var success = await _repository.SaveAsync(todoList, ct);

        return success ? Result<TodoListId>.Success(newId) : Result<TodoListId>.Failure(DomainError.InvalidOperation);
    }
};
