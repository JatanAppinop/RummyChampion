using System;
using System.Collections.Generic;

[System.Serializable]
public class PileShuffled
{
    public CardData topCard;
    
    // 🔹 ENHANCED PILE MANAGEMENT PROPERTIES
    public int totalCardsInPile;
    public int cardsRemaining;
    public string pileType; // "draw", "discard", "deck"
    public string shuffleReason; // "game_start", "deck_empty", "reshuffle_requested"
    public DateTime shuffleTime;
    public string shuffledBy; // player ID or "system"
    public string matchId;
    
    // Shuffle Information
    public bool wasShuffled;
    public int shuffleCount; // How many times this pile has been shuffled
    public string shuffleMethod; // "random", "secure_random", "deterministic"
    public string shuffleSeed; // For reproducible shuffles
    
    // Card Distribution
    public List<CardData> visibleCards; // Cards that all players can see
    public Dictionary<string, int> playerCardCounts; // How many cards each player has
    public List<string> cardOrder; // Order of cards (for debugging/verification)
    
    // Game State
    public string currentTurn; // Whose turn it is
    public bool isPileEmpty;
    public bool needsReshuffle;
    public CardData previousTopCard;
    
    // Security & Validation
    public string checksum; // For pile integrity verification
    public string signature; // Cryptographic signature
    public bool isValid;
    
    // 🔹 HELPER METHODS
    public bool CanDrawCard => !isPileEmpty && cardsRemaining > 0;
    public bool ShouldReshuffle => cardsRemaining <= 1 || needsReshuffle;
    public double FillPercentage => totalCardsInPile > 0 ? (double)cardsRemaining / totalCardsInPile : 0;
}

// 🔹 ADDITIONAL PILE-RELATED MODELS

[System.Serializable]
public class PileShuffleRequest
{
    public string matchId;
    public string playerId;
    public string pileType;
    public string reason;
    public bool includeDiscardPile;
}

[System.Serializable]
public class PileShuffleResponse : BaseModel<PileShuffled>
{
    
}

[System.Serializable]
public class PileStatusUpdate
{
    public string matchId;
    public string pileType;
    public int cardsRemaining;
    public CardData topCard;
    public bool needsReshuffle;
    public DateTime timestamp;
}

[System.Serializable]
public class DeckConfiguration
{
    public int totalCards = 52;
    public bool includeJokers = true;
    public int jokerCount = 2;
    public List<string> excludedCards; // Cards to remove from deck
    public string deckType; // "standard", "custom", "tournament"
    public bool allowDuplicates = false;
}

