using SettlersOfCrutan.Domain.Games; // For GameId, PlayerId

namespace SettlersOfCrutan.Domain.Core;
public class DomainError(string code, string message) : Error(code, message)
{
    // Generic / common
    public static DomainError NotFound => new("NotFound", "Resource not found.");
    public static DomainError InvalidOperation => new("InvalidOperation", "The operation is not valid in the current state.");
    public static DomainError Unauthorized => new("Unauthorized", "You do not have permission to perform this action.");
    public static DomainError Conflict => new("Conflict", "A conflict occurred with the current state of the resource.");
    public static DomainError InsufficientResources => new("InsufficientResources", "Not enough resources available to complete the operation.");

    // Game phase / turn
    public static DomainError GameAlreadyStarted => new("GameAlreadyStarted", "The game has already started and cannot be modified.");
    public static DomainError WrongTurn => new("WrongTurn", "It is not the correct turn for this player.");
    public static DomainError WrongTurnStatus => new("WrongTurnStatus", "The turn status is not currently active.");
    public static DomainError WrongGamePhase => new("WrongGamePhase", "Action cannot be performed at this turn step.");

    // Build piece availability
    public static DomainError MissingRoad => new("MissingRoad", "Player has no remaining roads to build.");
    public static DomainError MissingSettlement => new("MissingSettlement", "Player has no remaining settlements to build.");
    public static DomainError MissingCity => new("MissingCity", "Player has no remaining cities to build.");

    // Development cards
    public static DomainError TooManyDevelopmentCards => new("TooManyDevelopmentCards", "Can't have more than 3 development cards at a time.");
    public static DomainError InsufficientBankDevelopmentCards => new("InsufficientBankDevelopmentCards", "There are not enough bank development cards to satisfy the purchase.");

    // Ports / maritime
    public static DomainError Missing3to1Port => new("MaritimeTrade", "Player does not have a 3:1 maritime trade port");
    public static DomainError Missing2to1Port => new("MaritimeTrade", "Player does not have the matching 2:1 maritime trade port");

    // Dynamic / contextual
    public static DomainError PlayerNotFound(GameId gameId, PlayerId playerId) => new("PlayerNotFound", $"Player with id {playerId} not found in game {gameId}");
    public static DomainError UserNotInGame(GameId gameId) => new("UserNotInGame", $"Authenticated user is not a player in game {gameId}.");

    // Rolling dice
    public static DomainError CannotRollInCurrentPhase => new("Roll", "Cannot roll in the current game phase");

    // End turn
    public static DomainError InvalidEndTurn => new("EndTurn", "Invalid player index or direction");

    // Initial placement
    public static DomainError MissingInitialPieces => new("InitialPieces", "Missing initial pieces");

    // Discard half phase
    public static DomainError CannotDiscardInCurrentPhase => new("Discard", "Cannot discard in the current game phase");
    public static DomainError PlayerNotRequiredToDiscard => new("PlayerNotRequired", "Player not required to discard");
    public static DomainError InvalidDiscardsPayload => new("InvalidDiscardsPayload", "Invalid discards payload");
    public static DomainError IncorrectDiscardAmount => new("IncorrectDiscardAmount", "Incorrect discard amount");
    public static DomainError PlayerInsufficientResourcesToDiscard => new("InsufficientResources", "Player does not have required resources to discard");

    // Robber resolution
    public static DomainError CannotResolveRobberInCurrentPhase => new("Robber", "Cannot resolve robber in the current game phase");
    public static DomainError InvalidRobberMove => new("Robber", "Invalid robber move");
    public static DomainError VictimNotExposedToRobberHex => new("Robber", "Victim player is not exposed to the new robber hex");
    public static DomainError RobberVictimRequired => new("Robber", "At least one opponent has a settlement or city on this hex; you must choose one to steal from.");
    public static DomainError RobberVictimNotEligible => new("Robber", "That player is not an opponent with a settlement or city on this hex.");
    public static DomainError RobberVictimNotAllowed => new("Robber", "No opponent is on this hex; omit the victim — there is no one to steal from.");

    // Trade offer lifecycle (active offer)
    public static DomainError AnotherTradeOfferActive => new("TradeOffer", "Another trade offer is already active");

    // Trade offer creation / validation
    public static DomainError RequestedResourcesEmpty => new("TradeOffer", "Requested resources cannot be empty");
    public static DomainError OfferedResourcesEmpty => new("TradeOffer", "Offered resources cannot be empty");
    public static DomainError OfferedResourcesDuplicateTypes => new("TradeOffer", "Offered resources cannot contain duplicate resource types");
    public static DomainError RequestedResourcesDuplicateTypes => new("TradeOffer", "Requested resources cannot contain duplicate resource types");
    public static DomainError ProposerInsufficientResources => new("TradeOffer", "Proposer has insufficient resources to make trade offer");

    // Trade offer acceptance
    public static DomainError TradeOfferAlreadyAccepted => new("TradeOffer", "Trade offer already accepted");
    public static DomainError ProposerCannotAcceptOwnOffer => new("TradeOffer", "Proposer cannot accept their own trade offer");
    public static DomainError InvalidOriginalRequestor => new("TradeOffer", "Only the original proposer can be the original requestor");
    public static DomainError AcceptorInsufficientResources => new("TradeOffer", "Acceptor has insufficient resources to accept trade offer");

    // Dev card usage
    public static DomainError CannotPlayRoadBuilding => new("DevCard", "Cannot play Road Building");
    public static DomainError MissingMonopolyCard => new("DevCard", "Player does not have Monopoly card");
    public static DomainError MissingYearOfPlentyCard => new("DevCard", "Player does not have Year of Plenty card");
    public static DomainError MissingKnightCard => new("DevCard", "Player does not have Knight card");

    // Player setup
    public static Error ColorTaken => new("ColorTaken", "That color is already taken by another player.");
    internal static Error InvalidColor => new("InvalidColor", "Color chosen is invalid.");
    public static Error ColorMustSetBeforeReady => new("ColorMustSetBeforeReady", "Player must set a color before marking ready.");
    public static Error NameMustSetBeforeReady => new("NameMustSetBeforeReady", "Player must set a name before marking ready.");

    public static Error CannotEndTurnWithoutPlacingInitialSettlementAndRoad => new("CannotEndTurnWithoutPlacingInitialSettlementAndRoad", "Player must place initial road and settlement before ending turn.");
}
