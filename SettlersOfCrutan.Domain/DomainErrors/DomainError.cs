using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.DomainErrors;
public class DomainError(string code, string message) : Error(code, message)
{
    public static DomainError NotFound => new("NotFound", "Resource not found.");
    public static DomainError InvalidOperation => new("InvalidOperation", "The operation is not valid in the current state.");
    public static DomainError ValidationFailed => new("ValidationFailed", "One or more validation errors occurred.");
    public static DomainError Unauthorized => new("Unauthorized", "You do not have permission to perform this action.");
    public static DomainError Conflict => new("Conflict", "A conflict occurred with the current state of the resource.");
    public static DomainError InsufficientResources => new("InsufficientResources", "Not enough resources available to complete the operation.");
    public static DomainError GameAlreadyStarted => new("GameAlreadyStarted", "The game has already started and cannot be modified.");
    public static DomainError WrongTurn => new("WrongTurn", "It is not the correct turn for this player.");
    public static DomainError WrongTurnStatus => new("WrongTurnStatus", "The turn status is not currently active.");
    public static DomainError WrongTurnStep => new("WrongTurnStep", "Action cannot be performed at this turn step.");

}
