

using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Cards
{
    public string code;
    public string image;
    public CardImages images;
    public string value;
    public string suit;
    
    // 🔹 ENHANCED CARD PROPERTIES IN RESHUFFLE CONTEXT
    public int originalPosition; // Position before shuffle
    public int newPosition; // Position after shuffle
    public bool wasVisible; // Was this card visible before shuffle
    public string cardId; // Unique identifier
    public DateTime lastMoved;
}

[System.Serializable]
public class CardImages
{
    public string svg;
    public string png;
    
    // 🔹 ENHANCED IMAGE PROPERTIES
    public string thumbnail;
    public string highRes;
    public Dictionary<string, string> alternateFormats;
}

[System.Serializable]
public class PlayerDeck
{
    public string playerId;
    public List<Cards> drawPiles;
    public Cards topCard;
    
    // 🔹 ENHANCED PLAYER DECK PROPERTIES
    public string playerName;
    public int totalCards;
    public int visibleCards;
    public bool isShuffled;
    public DateTime lastShuffleTime;
    public string deckStatus; // "active", "empty", "needs_reshuffle", "shuffling"
    public int drawCount; // How many cards drawn from this deck
    public bool canDraw;
    public Cards previousTopCard;
}

// 🔹 NEW COMPREHENSIVE RESHUFFLE MODELS

[System.Serializable]
public class ReshuffleRequest
{
    public string matchId;
    public string requestedBy; // Player ID or "system"
    public string reason; // "deck_empty", "player_request", "game_rule", "security"
    public List<string> targetDecks; // Which decks to reshuffle
    public bool includeDiscardPile;
    public bool preserveTopCard;
    public string shuffleMethod; // "random", "secure", "deterministic"
    public DateTime requestTime;
}

[System.Serializable]
public class ReshuffleResponse : BaseModel<ReshuffleResult>
{
    
}

[System.Serializable]
public class ReshuffleResult
{
    public bool reshuffleCompleted;
    public string shuffleId; // Unique identifier for this shuffle
    public List<PlayerDeck> updatedDecks;
    public Cards newDiscardTop;
    public int totalCardsShuffled;
    public DateTime shuffleStartTime;
    public DateTime shuffleEndTime;
    public string shuffleMethod;
    public string shuffleSeed; // For reproducibility
    public bool wasSuccessful;
    public string errorMessage;
    
    // Security & Verification
    public string checksum; // Verify shuffle integrity
    public string signature; // Cryptographic signature
    public bool isVerified;
    
    // Game Impact
    public List<string> affectedPlayers;
    public bool gameCanContinue;
    public string nextAction; // What should happen next
    public Cards firstCardAfterShuffle;
    
    // Statistics
    public double shuffleTime; // Time taken in milliseconds
    public int entropyLevel; // Randomness quality measure
    public string shuffleQuality; // "excellent", "good", "fair", "poor"
}

[System.Serializable]
public class ShuffleValidation
{
    public bool canShuffle;
    public string validationMessage;
    public List<string> requirements; // What needs to be true for shuffle
    public List<string> restrictions; // What prevents shuffle
    public bool requiresPermission;
    public List<string> approversNeeded; // Who needs to approve this shuffle
    public int estimatedTime; // Estimated time in seconds
}

//[System.Serializable]
//public class DeckStatus
//{
//    public string deckId;
//    public string matchId;
//    public string deckType; // "main", "player", "discard", "special"
//    public int totalCards;
//    public int cardsRemaining;
//    public bool isEmpty;
//    public bool needsReshuffle;
//    public Cards topCard;
//    public Cards bottomCard; // For certain game modes
//    public DateTime lastAccessed;
//    public int accessCount; // How many times accessed
//    public bool isLocked; // Temporarily locked during operations
    
//    // Shuffle History
//    public List<ShuffleEvent> shuffleHistory;
//    public DateTime lastShuffleTime;
//    public int timesShuffled;
//    public string lastShuffleReason;
//    public string shufflePattern; // Pattern of when shuffles occur
    
//    // Security
//    public string integrityHash; // Hash of current card order
//    public bool integrityValid;
//    public DateTime lastIntegrityCheck;
//}

[System.Serializable]
public class ShuffleEvent
{
    public string shuffleId;
    public DateTime shuffleTime;
    public string reason;
    public string method;
    public int cardsShuffled;
    public string requestedBy;
    public bool wasSuccessful;
    public double timeTaken; // milliseconds
    public string qualityMetric;
}

[System.Serializable]
public class ShuffleConfiguration
{
    public string shuffleMethod; // "fisher_yates", "riffle", "overhand", "crypto_secure"
    public int minimumShuffles = 7; // Minimum number of shuffle iterations
    public bool verifyRandomness = true;
    public bool logShuffleEvents = true;
    public bool requireMultipleShuffles = false;
    public string randomSource; // "system", "hardware", "crypto"
    public int seedRotationInterval; // How often to change seed
    public bool auditableShuffles = true; // Keep audit trail
}

[System.Serializable]
public class ShuffleAnalytics
{
    public string matchId;
    public int totalShuffles;
    public double averageShuffleTime;
    public List<string> shuffleReasons;
    public Dictionary<string, int> shufflesByPlayer;
    public DateTime firstShuffle;
    public DateTime lastShuffle;
    public string mostCommonReason;
    public double shuffleFrequency; // Shuffles per hour
    public bool hasUnusualPattern; // Unusual shuffle patterns detected
}

[System.Serializable]
public class AutoShuffleSettings
{
    public bool enableAutoShuffle = true;
    public int reshuffleThreshold = 5; // Cards remaining when auto-shuffle triggers
    public bool shuffleOnGameStart = true;
    public bool shuffleOnRoundStart = false;
    public List<string> autoShuffleTriggers; // Events that trigger auto-shuffle
    public int maxAutoShuffles = 10; // Max auto-shuffles per game
    public bool requirePlayerConsent = false;
}
