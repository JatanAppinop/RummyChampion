// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
using System;
using System.Collections.Generic;

[System.Serializable]
public class CardData
{
    public string code;
    public string image;
    public Imagess images;
    public string value;
    public string suit;
    
    // 🔹 ENHANCED CARD PROPERTIES
    public string color; // "red", "black"
    public int numericValue; // Numeric value for calculations (A=1, J=11, Q=12, K=13)
    public int pointValue; // Points in Rummy (A=1, Face cards=10, others=face value)
    public bool isJoker;
    public bool isWildCard;
    public string cardType; // "number", "face", "ace", "joker"
    public int sortOrder; // For sorting cards in hand
    
    // Game Context
    public string cardId; // Unique identifier for this specific card instance
    public string ownerId; // Player who currently owns this card
    public string location; // "hand", "deck", "discard", "table"
    public DateTime? drawnTime;
    public DateTime? lastMoved;
    public bool isVisible; // Is this card visible to all players
    public bool isSelected; // Is this card currently selected by player
    
    // 🔹 HELPER METHODS
    public bool IsRed => suit == "HEARTS" || suit == "DIAMONDS";
    public bool IsBlack => suit == "CLUBS" || suit == "SPADES";
    public bool IsFaceCard => value == "JACK" || value == "QUEEN" || value == "KING";
    public bool IsAce => value == "ACE";
    public bool IsNumberCard => !IsFaceCard && !IsAce && !isJoker;
    
    public int GetNumericValue()
    {
        if (isJoker) return 0;
        if (IsAce) return 1;
        if (value == "JACK") return 11;
        if (value == "QUEEN") return 12;
        if (value == "KING") return 13;
        if (int.TryParse(value, out int num)) return num;
        return 0;
    }
    
    public int GetPointValue()
    {
        if (isJoker) return 0;
        if (IsFaceCard) return 10;
        if (IsAce) return 1;
        return GetNumericValue();
    }
}

[System.Serializable]
public class Imagess
{
    public string svg;
    public string png;
    
    // 🔹 ENHANCED IMAGE PROPERTIES
    public string thumbnail;
    public string highRes;
    public string webp;
    public string alt; // Alt text for accessibility
    public Dictionary<string, string> variants; // Different image variants
}

[System.Serializable]
public class PlayersData
{
    public string playerId;
    public List<CardData> cards;
    
    // 🔹 ENHANCED PLAYER CARD DATA
    public string playerName;
    public int totalCards;
    public int visibleCards;
    public int hiddenCards;
    public DateTime lastCardDrawn;
    public DateTime lastCardDropped;
    public bool hasValidSequence;
    public bool canDeclare;
    public int deadwoodPoints;
    public List<CardSequence> sequences;
    public List<CardSet> sets;
    
    // Game State
    public bool hasDropped;
    public bool isEliminated;
    public string playerStatus; // "active", "waiting", "dropped", "eliminated", "winner"
    public int turnPosition;
    public bool isCurrentTurn;
}

[System.Serializable]
public class PlayerCardsReponce
{
    public CardData lastCard;
    public List<PlayersData> players;
    
    // 🔹 ENHANCED RESPONSE DATA
    public string matchId;
    public DateTime timestamp;
    public string gamePhase;
    public string currentTurn;
    public int totalCardsInPlay;
    public DeckInfo deckInfo;
    public DiscardPileInfo discardInfo;
    public bool gameEnded;
    public string gameEndReason;
}

// 🔹 NEW COMPREHENSIVE CARD MODELS

[System.Serializable]
public class CardSequence
{
    public List<CardData> cards;
    public string sequenceType; // "pure", "impure", "set"
    public bool isValid;
    public int pointValue;
    public bool usesJoker;
    public string suit; // For sequences
    public string value; // For sets
}

[System.Serializable]
public class CardSet
{
    public List<CardData> cards;
    public string setValue; // The common value (e.g., "7")
    public bool isValid;
    public int pointValue;
    public bool usesJoker;
    public List<string> suits; // All suits in the set
}

[System.Serializable]
public class DeckInfo
{
    public int totalCards;
    public int cardsRemaining;
    public bool isEmpty;
    public bool needsReshuffle;
    public CardData topCard; // Only if visible
    public int timesShuffled;
    public DateTime lastShuffleTime;
}

[System.Serializable]
public class DiscardPileInfo
{
    public CardData topCard;
    public int totalCards;
    public List<CardData> recentCards; // Last few cards for display
    public string lastDroppedBy;
    public DateTime lastDropTime;
    public bool canPickUp;
}

[System.Serializable]
public class CardGameState
{
    public string matchId;
    public List<PlayersData> allPlayers;
    public DeckInfo deck;
    public DiscardPileInfo discardPile;
    public string currentPhase; // "dealing", "playing", "declaring", "finished"
    public string currentPlayer;
    public int currentTurn;
    public DateTime gameStartTime;
    public DateTime? gameEndTime;
    public string gameMode;
    public string gameType;
}

[System.Serializable]
public class CardMovement
{
    public string cardId;
    public string playerId;
    public string fromLocation;
    public string toLocation;
    public DateTime moveTime;
    public string moveReason; // "draw", "discard", "deal", "shuffle"
    public bool isVisible;
    public string moveType; // "normal", "forced", "penalty"
}

[System.Serializable]
public class HandAnalysis
{
    public string playerId;
    public List<CardData> hand;
    public List<CardSequence> validSequences;
    public List<CardSet> validSets;
    public List<CardData> deadwoodCards;
    public int deadwoodPoints;
    public bool hasValidDeclaration;
    public bool hasPureSequence;
    public int minimumPointsNeeded;
    public List<string> recommendations; // Suggested moves
}

[System.Serializable]
public class CardDealRequest
{
    public string matchId;
    public List<string> playerIds;
    public int cardsPerPlayer;
    public bool shuffleDeck;
    public string dealReason; // "game_start", "new_deal", "reshuffle"
}

[System.Serializable]
public class CardDealResponse : BaseModel<CardDealResult>
{
    
}

[System.Serializable]
public class CardDealResult
{
    public bool dealCompleted;
    public Dictionary<string, List<CardData>> playerHands;
    public CardData discardPileStart;
    public DeckInfo remainingDeck;
    public DateTime dealTime;
    public string dealId;
}

