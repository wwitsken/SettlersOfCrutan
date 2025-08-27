using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Todos;

public record TodoListId : BaseId;
public class TodoList(TodoListId id, string name, List<string> todos) : AggregateRoot<TodoListId>
{
    public override TodoListId Id { get; init; } = id;
    public string Name { get; set; } = name;
    public List<string> Todos { get; private set; } = todos;
}
