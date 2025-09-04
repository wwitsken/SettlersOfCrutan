using SettlersOfCrutan.Domain.Core;

namespace SettlersOfCrutan.Domain.Games.DomainEvents;
public record DiceRolledDomainEvent(GameId Id, int Dice1, int Dice2) : IDomainEvent;
