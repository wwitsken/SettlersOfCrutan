using SettlersOfCrutan.Application.Abstractions;
using SettlersOfCrutan.Domain.Todos;

namespace SettlersOfCrutan.Application.Todos;
public interface ITodoListRepository : IRepository<TodoList, TodoListId>;