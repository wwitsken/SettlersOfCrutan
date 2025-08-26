using SettlersOfCrutan.Application.Todos;
using SettlersOfCrutan.Domain.Todos;

namespace SettlersOfCrutan.Infrastructure.Redis.Repositories;
public class RedisTodoListRepository(RedisRepository<TodoList, TodoListId> inner) : ITodoListRepository
{
    private readonly RedisRepository<TodoList, TodoListId> _inner = inner;

    public Task<TodoList?> GetAsync(TodoListId id, CancellationToken ct = default) => _inner.GetAsync(id, ct);
    public Task<bool> SaveAsync(TodoList aggregate, CancellationToken ct = default) => _inner.SaveAsync(aggregate, ct);
    public Task<bool> DeleteAsync(TodoListId id, CancellationToken ct = default) => _inner.DeleteAsync(id, ct);
}
