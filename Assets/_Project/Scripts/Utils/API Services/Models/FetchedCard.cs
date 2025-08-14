using System;
using System.Collections.Generic;

[System.Serializable]
public class FetchedCardData
{
    public string requestedBy;
    public CardData card;
    
    // 🔹 ENHANCED CARD FETCH PROPERTIES
    public string playerName;
    public string matchId;
    public DateTime fetchTime;
    public int turnNumber;
    public string fetchSource; // "deck", "discard_pile", "special_deck"
    
    // Card Information
    public string cardPosition; // Position in the source pile
    public bool wasTopCard;
    public bool wasVisibleCard;
    public string cardOrigin; // "original_deck", "shuffled_deck", "player_drop"
    
    // Game Context
    public string gamePhase; // "dealing", "playing", "declaring"
    public int cardsInHandAfterFetch;
    public bool triggersAction; // Does this fetch trigger any special action
    public bool completesSequence; // Does this card complete a sequence
    public bool isJoker;
    public bool isPureSequence;
    
    // Fetch Validation
    public bool isValidFetch;
    public string validationError;
    public bool requiresPermission; // Some cards might need special permission
    public string fetchReason; // "normal_turn", "forced_draw", "penalty_draw", "bonus_draw"
    
    // Turn Management
    public string nextPlayer;
    public bool endsTurn;
    public bool allowsDiscard; // Can player discard after this fetch
    public double timeSpentOnFetch; // Time taken to make the fetch decision
    public DateTime turnStartTime;
    public DateTime turnEndTime;
    public int timeRemainingForTurn; // seconds
    
    // Deck Status After Fetch
    public int cardsRemainingInSource;
    public CardData newTopCard; // New top card after this fetch
    public bool deckNeedsReshuffle;
    public bool deckIsEmpty;
    
    // Points & Scoring
    public int cardPointValue;
    public bool affectsDeadwood;
    public int deadwoodAfterFetch;
    public bool createsNewSequence;
    public bool breaksExistingSequence;
    
    // Security & Validation
    public string checksum; // For fetch verification
    public string signature; // Cryptographic signature
    public bool isSecureFetch;
    
    // 🔹 HELPER METHODS
    public bool IsValidFetch => isValidFetch && string.IsNullOrEmpty(validationError);
    public bool IsFromDeck => fetchSource == "deck";
    public bool IsFromDiscardPile => fetchSource == "discard_pile";
    public bool IsJokerCard => isJoker || (card != null && (card.value == "JOKER" || card.code.Contains("JOKER")));
    public TimeSpan FetchDuration => turnEndTime > turnStartTime ? turnEndTime - turnStartTime : TimeSpan.Zero;
    public bool IsSlowFetch => FetchDuration.TotalSeconds > 10; // Taking more than 10 seconds
}

// 🔹 ADDITIONAL CARD FETCH MODELS

[System.Serializable]
public class CardFetchRequest
{
    public string playerId;
    public string matchId;
    public string fetchSource; // "deck", "discard_pile"
    public string reason; // "normal_turn", "forced_draw", etc.
    public bool skipValidation; // For admin/debug purposes
    public DateTime requestTime;
}

[System.Serializable]
public class CardFetchResponse : BaseModel<FetchedCardData>
{
    
}

[System.Serializable]
public class CardFetchValidation
{
    public bool canFetch;
    public string validationMessage;
    public List<string> availableSources; // "deck", "discard_pile", etc.
    public List<string> restrictions;
    public bool requiresTurnStart;
    public int maxFetchTime; // seconds
    public bool autoFetch; // Should auto-fetch if only one option
}

[System.Serializable]
public class DeckStatus
{
    public string matchId;
    public int cardsRemaining;
    public CardData topCard; // Only visible if it should be
    public bool isEmpty;
    public bool needsReshuffle;
    public DateTime lastShuffleTime;
    public int totalFetches; // Total cards fetched this game
    public string deckType; // "main", "secondary", "joker"
}

[System.Serializable]
public class CardDrawHistory
{
    public string matchId;
    public List<FetchedCardData> recentDraws;
    public Dictionary<string, int> playerDrawCounts;
    public DateTime lastDrawTime;
    public string lastDrawer;
    public int totalDrawsThisGame;
}

[System.Serializable]
public class CardPickOptions
{
    public bool canPickFromDeck;
    public bool canPickFromDiscard;
    public CardData discardPileTop;
    public int deckCardsRemaining;
    public string recommendedAction;
    public List<string> availableActions;
    public int timeLimit; // seconds to make choice
}

