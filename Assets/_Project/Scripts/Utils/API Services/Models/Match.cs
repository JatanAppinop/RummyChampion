using System;
using System.Collections.Generic;

[Serializable]
public class MatchData
{
    public string _id;
    public string tableId;
    public List<string> players;
    public DateTime gameStartedDate;
    public DateTime? gameEndedDate;
    public List<object> winner;
    public int __v;
    
    // 🔹 ENHANCED MATCH PROPERTIES
    public string matchStatus; // "waiting", "in_progress", "completed", "cancelled"
    public string gameMode; // "Pool", "Deals", "Points"
    public string gameType; // "Pool101", "Pool201", "2Player", "4Player", "6Player"
    public double entryFee;
    public double totalPrizePool;
    public double rakeAmount;
    public double rakePercentage;
    public int maxPlayers;
    public int currentPlayers;
    
    // Game Configuration
    public int pointValue; // Value per point in Points Rummy
    public int totalDeals; // For Deals Rummy
    public int eliminationThreshold; // For Pool Rummy (101 or 201)
    public int currentDeal; // Current deal number for Deals Rummy
    public string currentTurn; // Player ID whose turn it is
    public string gamePhase; // "dealing", "playing", "declaring", "finished"
    
    // Player Statistics
    public Dictionary<string, int> playerScores;
    public Dictionary<string, int> cumulativeScores;
    public Dictionary<string, int> dealsWon;
    public Dictionary<string, bool> droppedPlayers;
    public Dictionary<string, bool> eliminatedPlayers;
    public Dictionary<string, DateTime> playerJoinTimes;
    public Dictionary<string, DateTime> playerLastActionTimes;
    
    // Game History
    public List<string> gameHistory; // JSON strings of important game events
    public List<string> chatHistory;
    public string lastCardPlayed;
    public string lastPlayerAction;
    public DateTime lastActionTime;
    
    // Tournament/Contest Information
    public string contestId;
    public string tournamentId;
    public bool isPrivate;
    public string inviteCode;
    public bool isBot; // Contains bot players
    
    // Technical Information
    public string serverRegion;
    public string gameVersion;
    public Dictionary<string, object> metadata; // Additional flexible data
}

[Serializable]
class Match : BaseModel<MatchData>
{
    
}

// 🔹 ADDITIONAL MATCH-RELATED MODELS

[Serializable]
public class MatchJoinRequest
{
    public string playerId;
    public string playerName;
    public string tableId;
    public double walletBalance;
    public string inviteCode;
}

[Serializable]
public class MatchJoinResponse : BaseModel<MatchJoinData>
{
    
}

[Serializable]
public class MatchJoinData
{
    public string matchId;
    public string seatPosition;
    public List<string> currentPlayers;
    public bool gameStarted;
    public string waitingMessage;
    public int playersNeeded;
}

[Serializable]
public class MatchLeaveRequest
{
    public string playerId;
    public string matchId;
    public string reason; // "manual", "timeout", "disconnection", "elimination"
}

[Serializable]
public class MatchStatusUpdate
{
    public string matchId;
    public string status;
    public List<string> activePlayers;
    public string currentTurn;
    public string gamePhase;
    public DateTime timestamp;
}
