using SettlersOfCrutan.Domain.Core;
using SettlersOfCrutan.Domain.DomainErrors;
using SettlersOfCrutan.Domain.Todos;

namespace SettlersOfCrutan.Application.Todos;
public record GetTodoListByIdQuery(TodoListId Id);

public class GetTodoListByIdQueryHandler
{
    private readonly ITodoListRepository _repository;

    public GetTodoListByIdQueryHandler(ITodoListRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TodoList>> Handle(GetTodoListByIdQuery query, CancellationToken ct = default)
    {
        TodoList? todoList = await _repository.GetAsync(query.Id, ct);

        return todoList is not null ? Result<TodoList>.Success(todoList) : Result<TodoList>.Failure(DomainError.InvalidOperation);
    }
};
