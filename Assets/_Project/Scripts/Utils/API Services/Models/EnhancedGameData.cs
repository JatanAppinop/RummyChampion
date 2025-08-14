using System;
using System.Collections.Generic;

// 🔹 ENHANCED RUMMY GAME DATA MODELS

[System.Serializable]
public class PlayerReadyData
{
    public string playerId;
    public string playerName;
    public string matchId;
    public string gameMode; // "Pool", "Deals", "Points"
    public string gameType; // "Pool101", "Pool201", "2Deal", etc.
    public bool isReady;
    public DateTime readyTime;
    public string playerStatus; // "waiting", "ready", "in_game"
    public int currentPlayers;
    public int maxPlayers;
    public string tableId;
    public double walletBalance;
    public string clientVersion;
    public string deviceInfo;
}

[System.Serializable]
public class PlayerDroppedData
{
    public string playerId;
    public string playerName;
    public int penaltyPoints;
    public bool hasPickedCard;
    public int cumulativeScore;
    public string gameMode;
    public string matchId;
}

[System.Serializable]
public class PlayerEliminatedData
{
    public string playerId;
    public string playerName;
    public int eliminationScore;
    public int threshold;
    public string gameMode;
    public string matchId;
    public int remainingPlayers;
}

[System.Serializable]
public class DealCompletedData
{
    public string winnerId;
    public string winnerName;
    public int dealNumber;
    public int totalDeals;
    public Dictionary<string, int> playerScores;
    public Dictionary<string, int> cumulativeScores;
    public Dictionary<string, int> dealsWon;
    public string matchId;
    public bool isFinalDeal;
}

[System.Serializable]
public class DealStartedData
{
    public int dealNumber;
    public int totalDeals;
    public string matchId;
    public List<string> activePlayers;
    public Dictionary<string, int> cumulativeScores;
}

[System.Serializable]
public class PoolGameEndedData
{
    public string winnerId;
    public string winnerName;
    public int winningAmount;
    public string poolType; // "Pool101" or "Pool201"
    public Dictionary<string, int> finalScores;
    public List<string> eliminatedPlayers;
    public string matchId;
}

[System.Serializable]
public class CumulativeScoreData
{
    public string playerId;
    public int scoreThisRound;
    public int cumulativeScore;
    public bool isEliminated;
    public string gameMode;
    public string matchId;
}

[System.Serializable]
public class GameModeChangedData
{
    public string gameMode; // "Pool", "Deals", "Points"
    public string gameType; // "Pool101", "Pool201", etc.
    public int totalDeals; // For Deals Rummy
    public int eliminationThreshold; // For Pool Rummy
    public string matchId;
}

[System.Serializable]
public class ActivePlayersData
{
    public List<string> activePlayers;
    public List<string> droppedPlayers;
    public List<string> eliminatedPlayers;
    public int activeCount;
    public string gameMode;
    public string matchId;
}

// 🔹 HELPER CLASSES FOR GAME STATE MANAGEMENT

[System.Serializable]
public class PlayerGameState
{
    public string playerId;
    public bool hasDropped;
    public bool isEliminated;
    public int cumulativeScore;
    public int dealsWon;
    public bool hasPickedCardThisTurn;
}

[System.Serializable]
public class GameStateData
{
    public string gameMode;
    public string gameType;
    public int currentDeal;
    public int totalDeals;
    public List<PlayerGameState> playerStates;
    public string currentPhase;
    public string matchId;
} 