using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class DealScore
{
    public string playerId;
    public int dealNumber;
    public int score;
    public bool wonDeal;
    
    public DealScore(string playerId, int dealNumber, int score, bool wonDeal)
    {
        this.playerId = playerId;
        this.dealNumber = dealNumber;
        this.score = score;
        this.wonDeal = wonDeal;
    }
}

public class DealsRummyManager : MonoBehaviour
{
    [Header("Deal Configuration")]
    public int totalDeals = 2; // Default number of deals
    public int currentDeal = 1;
    
    [Header("Deal Tracking")]
    public List<DealScore> dealHistory = new List<DealScore>();
    public Dictionary<string, int> playerDealsWon = new Dictionary<string, int>();
    public Dictionary<string, int> playerCumulativeScores = new Dictionary<string, int>();
    
    private GameManager gameManager;
    private GameModeIndicator gameModeIndicator;
    
    public static DealsRummyManager instance;
    
    // Events
    public Action<int> OnDealStarted;
    public Action<int, DealScore> OnDealCompleted;
    public Action<List<DealScore>> OnAllDealsCompleted;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            gameManager = FindObjectOfType<GameManager>();
            gameModeIndicator = FindObjectOfType<GameModeIndicator>();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void InitializeDealsRummy(int numberOfDeals)
    {
        totalDeals = numberOfDeals;
        currentDeal = 1;
        dealHistory.Clear();
        playerDealsWon.Clear();
        playerCumulativeScores.Clear();
        
        // Initialize player tracking
        foreach (Player player in gameManager.playerList)
        {
            playerDealsWon[player.playerId] = 0;
            playerCumulativeScores[player.playerId] = 0;
        }
        
        Debug.Log($"Initialized Deals Rummy with {totalDeals} deals");
        OnDealStarted?.Invoke(currentDeal);
    }
    
    public void CompleteDeal(Player winner, List<Player> allPlayers)
    {
        Debug.Log($"Completing deal {currentDeal}");
        
        // Record scores for all players in this deal
        foreach (Player player in allPlayers)
        {
            int dealScore = player.GetFinalScore();
            bool wonThisDeal = (player == winner);
            
            DealScore score = new DealScore(player.playerId, currentDeal, dealScore, wonThisDeal);
            dealHistory.Add(score);
            
            // Update cumulative scores
            if (!playerCumulativeScores.ContainsKey(player.playerId))
                playerCumulativeScores[player.playerId] = 0;
            
            playerCumulativeScores[player.playerId] += dealScore;
            
            // Update deals won
            if (wonThisDeal)
            {
                if (!playerDealsWon.ContainsKey(player.playerId))
                    playerDealsWon[player.playerId] = 0;
                
                playerDealsWon[player.playerId]++;
                player.IncrementDealsWon();
            }
            
            // Update player's cumulative score
            player.AddToCumulativeScore(dealScore);
        }
        
        OnDealCompleted?.Invoke(currentDeal, dealHistory.Last());
        
        // Check if all deals are complete
        if (currentDeal >= totalDeals)
        {
            CompleteAllDeals();
        }
        else
        {
            StartNextDeal();
        }
    }
    
    private void StartNextDeal()
    {
        currentDeal++;
        Debug.Log($"Starting deal {currentDeal} of {totalDeals}");
        
        // Reset players for new deal
        foreach (Player player in gameManager.playerList)
        {
            player.ResetForNewDeal();
        }
        
        // Notify UI to update
        gameModeIndicator?.OnDealChanged();
        
        OnDealStarted?.Invoke(currentDeal);
        
        // Start new round
        gameManager.StartNewDeal();
    }
    
    private void CompleteAllDeals()
    {
        Debug.Log("All deals completed!");
        
        // Determine final winner
        Player finalWinner = DetermineFinalWinner();
        
        OnAllDealsCompleted?.Invoke(dealHistory);
        
        // Show final results
        ShowFinalResults(finalWinner);
    }
    
    private Player DetermineFinalWinner()
    {
        // Winner is the player who won the most deals
        // In case of tie, winner is the player with lowest cumulative score
        
        string winnerId = "";
        int maxDealsWon = 0;
        int lowestScore = int.MaxValue;
        
        foreach (var kvp in playerDealsWon)
        {
            string playerId = kvp.Key;
            int dealsWon = kvp.Value;
            int cumulativeScore = playerCumulativeScores.ContainsKey(playerId) ? playerCumulativeScores[playerId] : 0;
            
            if (dealsWon > maxDealsWon || (dealsWon == maxDealsWon && cumulativeScore < lowestScore))
            {
                maxDealsWon = dealsWon;
                lowestScore = cumulativeScore;
                winnerId = playerId;
            }
        }
        
        return gameManager.playerList.Find(p => p.playerId == winnerId);
    }
    
    private void ShowFinalResults(Player winner)
    {
        if (winner != null)
        {
            int winningAmount = gameManager.GetDealsWinningAmount();
            FullscreenTextMessage.instance.ShowText($"{winner.name} Wins!\n₹{winningAmount}", 5f);
            
            // Award winnings to winner
            winner.AddScore(winningAmount);
        }
        
        // Show detailed results
        DisplayDealsSummary();
    }
    
    private void DisplayDealsSummary()
    {
        string summary = "DEALS SUMMARY\n\n";
        
        foreach (Player player in gameManager.playerList)
        {
            int dealsWon = playerDealsWon.ContainsKey(player.playerId) ? playerDealsWon[player.playerId] : 0;
            int cumulativeScore = playerCumulativeScores.ContainsKey(player.playerId) ? playerCumulativeScores[player.playerId] : 0;
            
            summary += $"{player.name}: {dealsWon} deals won, {cumulativeScore} total points\n";
        }
        
        Debug.Log(summary);
    }
    
    // Public getters for UI
    public int GetCurrentDealNumber() => currentDeal;
    public int GetTotalDeals() => totalDeals;
    public int GetPlayerDealsWon(string playerId) => playerDealsWon.ContainsKey(playerId) ? playerDealsWon[playerId] : 0;
    public int GetPlayerCumulativeScore(string playerId) => playerCumulativeScores.ContainsKey(playerId) ? playerCumulativeScores[playerId] : 0;
    
    public List<DealScore> GetPlayerDealHistory(string playerId)
    {
        return dealHistory.Where(d => d.playerId == playerId).ToList();
    }
    
    public bool IsLastDeal() => currentDeal >= totalDeals;
} 