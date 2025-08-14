using System;
using System.Collections.Generic;

[System.Serializable]
public class CardDropped
{
    public string playerId;
    public string cardCode;
    
    // 🔹 ENHANCED CARD DROP PROPERTIES
    public string playerName;
    public string matchId;
    public DateTime dropTime;
    public int turnNumber;
    public string dropPosition; // "top", "bottom", "middle" of discard pile
    
    // Card Information
    public CardData droppedCard;
    public string dropReason; // "normal_play", "forced_drop", "timeout", "invalid_hold"
    public bool wasDrawnThisTurn;
    public string sourceLocation; // "hand", "picked_from_deck", "picked_from_discard"
    
    // Game Context
    public string gamePhase; // "playing", "declaring", "finishing"
    public int cardsInHandAfterDrop;
    public bool isDeclaringTurn;
    public bool isPenaltyDrop; // Drop due to penalty or invalid play
    
    // Validation & Security
    public bool isValidDrop;
    public string validationError;
    public List<string> availableCards; // Cards player had when dropping
    public string checksum; // For drop verification
    
    // Turn Management
    public string nextPlayer;
    public bool endsTurn;
    public double timeSpentOnDrop; // Time taken to make the drop decision
    public DateTime turnStartTime;
    public DateTime turnEndTime;
    
    // Points & Scoring (for certain game modes)
    public int pointValue; // Point value of dropped card
    public bool affectsScore;
    public int playerScoreAfterDrop;
    
    // 🔹 HELPER METHODS
    public bool IsValidDrop => isValidDrop && string.IsNullOrEmpty(validationError);
    public bool IsTimeoutDrop => dropReason == "timeout";
    public bool IsForcedDrop => dropReason == "forced_drop" || isPenaltyDrop;
    public TimeSpan TurnDuration => turnEndTime > turnStartTime ? turnEndTime - turnStartTime : TimeSpan.Zero;
}

// 🔹 ADDITIONAL CARD DROP MODELS

[System.Serializable]
public class CardDropRequest
{
    public string playerId;
    public string matchId;
    public string cardCode;
    public string dropReason;
    public bool isDeclaringTurn;
    public DateTime requestTime;
}

[System.Serializable]
public class CardDropResponse : BaseModel<CardDropResult>
{
    
}

[System.Serializable]
public class CardDropResult
{
    public bool dropAccepted;
    public string resultMessage;
    public CardDropped dropDetails;
    public string nextPlayer;
    public int cardsRemainingInHand;
    public bool gameEnded;
    public string gameEndReason;
}

[System.Serializable]
public class CardDropValidation
{
    public bool canDrop;
    public string validationMessage;
    public List<string> droppableCards;
    public List<string> restrictions;
    public bool requiresDeclaration;
    public int maxDropTime; // seconds
}

[System.Serializable]
public class DiscardPileStatus
{
    public string matchId;
    public CardData topCard;
    public int totalCardsInPile;
    public List<CardData> recentDrops; // Last few cards for display
    public string lastDroppedBy;
    public DateTime lastDropTime;
    public bool canPickFromDiscard;
}

