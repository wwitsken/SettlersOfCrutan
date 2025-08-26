using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Todos;

public record TodoId : BaseId;
public class Todo(TodoId id, string title, string message, bool isDone) : Entity<TodoId>
{
    public override TodoId Id { get; } = id;
    public string Title { get; private set; } = title;
    public string Message { get; private set; } = message;
    public bool IsDone { get; private set; } = isDone;
}
