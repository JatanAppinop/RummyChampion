using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

[System.Serializable]
public class PlayerCardsArea
{
    public Image playerAvatar;
    public Text playerScore;
    public Transform startAnimPoint;
    public MeldArea meldArea;
    public DeadwoodArea deadwoodArea;
    public Player player;
}
public class RoundEndScreen : IHUD
{
    public static RoundEndScreen instance;
    public PlayerCardsArea[] playerCardsAreas;
    private int totalScore;
    public PlayerCardsArea winner;
    public PlayerCardsArea loser;

    protected override void Awake()
    {
        base.Awake();
        instance = this;
        gameManager = FindObjectOfType<GameManager>();
    }


    public void EndRound(Hand[] playersHand)
    {

        foreach (var c in playerCardsAreas)
        {
            // Destroy all children inside meldArea
            foreach (Transform child in c.meldArea.transform)
            {
                Destroy(child.gameObject);
            }

            // Destroy all children inside deadwoodArea
            foreach (Transform child in c.deadwoodArea.transform)
            {
                Destroy(child.gameObject);
            }
        }

        TurnOnScreen();

        StartCoroutine(EndRoundAnimation(playersHand));
    }

    private IEnumerator EndRoundAnimation(Hand[] playersHand)
    {
        if (gameManager.tableData.gameMode.Contains("Deals"))
            gameManager.currentPlayer.gameFinished = true;
        // Assign player details to UI
        for (int i = 0; i < playersHand.Length; i++)
        {
            Hand hand = playersHand[i];
            PlayerCardsArea playerCardsArea = playerCardsAreas[i];
            playerCardsArea.player = hand.playerOfThisHand;
            playerCardsArea.playerAvatar.sprite = hand.playerOfThisHand.GetPlayerAvatar();
            playerCardsArea.playerScore.text = hand.playerOfThisHand.GetScore().ToString();
        }

        // Animate cards for all players
        for (int i = 0; i < playersHand.Length; i++)
        {
            Hand hand = playersHand[i];
            List<Card> playerMeld = hand.GetSequencedCards();
            List<Card> playerDeadwood = hand.GetNotSequencedCards();
            PlayerCardsArea playerCardsArea = playerCardsAreas[i];

            yield return playerCardsArea.meldArea.AnimateCardToArea(playerMeld, playerCardsArea.startAnimPoint);
            yield return playerCardsArea.deadwoodArea.AnimateCardToArea(playerDeadwood, playerCardsArea.startAnimPoint);
        }

        yield return new WaitForSeconds(0.5f);


        // Determine winner and loser
        AssignWinnerAndLoserOfTheMatch(playersHand[0].playerOfThisHand);
        //if (gameManager.winType == WinType.Knock)
        //{
        //    yield return winner.meldArea.CalculateLayoff(loser.deadwoodArea);
        //}

        yield return winner.deadwoodArea.AnimateDeadwoodCards();
        yield return loser.deadwoodArea.AnimateDeadwoodCards();
        yield return Constants.delayAfterLayoutCardAnim;

        // Calculate Points & Show Winning Amount
        CalculatePoints();

        yield return new WaitForSeconds(1);
        TurnOffScreen();
        //AddAndShowBonusRoundPoints();    before will block this line
        yield return new WaitForSeconds(1);



        // Check if the match is over and take appropriate action
        if (CheckIfMatchIsOver())
        {
            GameManager.instance.CloseThisGame();
            winner.player.WinGame();
            loser.player.LoseGame();
        }


        //Screen.orientation = ScreenOrientation.Portrait;
        //SceneManager.LoadScene((int)Scenes.MainMenu);
    }
    public async void RummyGameResults(int win, int los)
    {

        RummyGameResult data = new RummyGameResult();
        data.WinnerId = winner.player.playerId;
        data.LoserId = loser.player.playerId;// Fixed LoserId assignment
        data.WinnerPoints = win;
        data.LoserPoints = los;

        await RummySocketServer.Instance.SendEvent(RummySocketEvents.game_ended, data.ToDictionary());
    }
    public class RummyGameResult
    {
        public string WinnerId { get; set; }
        public string LoserId { get; set; }
        public int WinnerPoints { get; set; }
        public int LoserPoints { get; set; }

        public Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>
        {
            { "winnerId", WinnerId },
            { "loserId", LoserId },
            { "winnerPoints", WinnerPoints.ToString() },
            { "loserPoints", LoserPoints.ToString() }
        };
        }
    }
    // Creating the result object


    // Convert to Dictionary<string, string> before sending

    private bool CheckIfMatchIsOver()
    {
        if (gameManager.tableData.gameMode.Contains("101"))
        {
            // Pool Rummy: Check if only one player remains
            return gameManager.CheckIfOnlyOnePlayerRemains();
        }
        return winner.player.CheckIfPlayerHasEnoughScoreToWinGame();
    }


    private void AssignWinnerAndLoserOfTheMatch(Player firstPlayer)
    {
        int winnerID = firstPlayer == gameManager.GetWinnerOfMatch() ? 0 : 1;
        int loserID = (winnerID + 1) % 2;

        winner = playerCardsAreas[winnerID];
        loser = playerCardsAreas[loserID];
    }

    private void CalculatePoints()
    {
        int winnerDeadwoodScore = winner.deadwoodArea.CalculateDeadwood();
        int loserDeadwoodScore = loser.deadwoodArea.CalculateDeadwood();
        totalScore = loserDeadwoodScore - winnerDeadwoodScore;

        FullscreenTextMessage.instance.ShowText(winner.player.userData.username + " Won", 5f);

        // 🔹 Winning amount calculation based on game type
        double winningAmount = 0;

        if (gameManager.tableData.gameMode.Contains("Points"))
        {
            // Points Rummy: Winning = Points Earned × Per Point Value
            double perPointValue = gameManager.GetPerPointValue();
            winningAmount = loserDeadwoodScore * perPointValue;


            // 🔹 Deduct platform fee
            int platformFee = gameManager.GetPlatformFee((int)winningAmount);
            winningAmount -= platformFee;

            // 🔹 Ensure winnings are never negative
            winningAmount = Mathf.Max(Constants.MINIMUM_WIN_AMOUNT, (int)winningAmount);

            // 🔹 Add winnings to the winner
            winner.player.AddScore((int)winningAmount);

            // 🔹 Show winnings on screen
            FullscreenTextMessage.instance.ShowText(winner.player.userData.username + " Wins ₹" + (int)winningAmount, 5f);

            RummyGameResults(winnerDeadwoodScore, loserDeadwoodScore);
            Screen.orientation = ScreenOrientation.Portrait;
            SceneManager.LoadScene((int)Scenes.MainMenu);
        }
        else if (gameManager.tableData.gameMode.Contains("Deals"))
        {
            // 🔹 ENHANCED DEALS RUMMY LOGIC
            HandleDealsRummyEnd(winnerDeadwoodScore, loserDeadwoodScore);
        }
        else if (gameManager.tableData.gameMode.Contains("Pool"))
        {
            // 🔹 ENHANCED POOL RUMMY LOGIC
            HandlePoolRummyEnd(winnerDeadwoodScore, loserDeadwoodScore);
        }



    }
    private bool CheckIfIsItUndercut(int totalPoints)
    {
        return totalPoints <= 0;
    }
    private void SwapPlayers()
    {
        PlayerCardsArea temp = winner;
        winner = loser;
        loser = temp;
    }

    private void AddAndShowBonusRoundPoints()
    {
        Bid bid = gameManager.bid;
        if (bid.bonusPoints > 0)
        {
            FullscreenTextMessage.instance.ShowText("Round bonus +" + bid.bonusPoints + " score");
        }
        winner.player.AddScore(bid.bonusPoints);
        RefreshScore();

        // 🔹 Show final winning amount message
        FullscreenTextMessage.instance.ShowText(winner.player.userData.username + " Wins ₹" + winner.player.GetScore(), 5f);
    }

    public void TransferPoints(ScoreBall ball)
    {
        Vector3 destinationPosition = winner.playerScore.transform.position;
        ball.ShowWithMoveOnlyAnim(totalScore, destinationPosition, RefreshScore);
    }
    void RefreshScore()
    {
        winner.playerScore.text = winner.player.GetScore().ToString();
    }
    
    // 🔹 NEW ENHANCED GAME MODE HANDLERS
    
    private void HandleDealsRummyEnd(int winnerDeadwoodScore, int loserDeadwoodScore)
    {
        Debug.Log("[RoundEndScreen] Handling Deals Rummy round end");
        
        // Update cumulative scores
        int scoreDifference = loserDeadwoodScore - winnerDeadwoodScore;
        
        // Winner gets 0 points, loser gets the difference
        loser.player.AddToCumulativeScore(scoreDifference);
        
        // Update deals won count for winner
        winner.player.IncrementDealsWon();
        
        // Complete the deal through game manager
        gameManager.CompleteDeal(winner.player);
        
        // 🔹 LIVE FUNCTIONALITY: Check if this was the final deal
        if (gameManager.GetCurrentDealNumber() >= gameManager.GetTotalDeals())
        {
            HandleFinalDealsRummyCompletion();
        }
        else
        {
            HandleDealsRummyContinuation();
        }
    }
    
    // 🔹 NEW: Handle final deal completion in Deals Rummy
    private void HandleFinalDealsRummyCompletion()
    {
        Debug.Log("[RoundEndScreen] Final deal completed - determining Deals Rummy winner");
        
        // Find overall winner based on deals won and cumulative score
        Player overallWinner = DetermineDealsRummyWinner();
        
        if (overallWinner != null)
        {
            int winningAmount = GetDealsRummyWinningAmount();
            overallWinner.AddScore(winningAmount);
            
            FullscreenTextMessage.instance.ShowText(
                $"{overallWinner.userData.username} Wins Deals Rummy!\n₹{winningAmount}", 5f);
            
            Debug.Log($"Deals Rummy completed. Winner: {overallWinner.name}");
        }
        
        // End the game
        gameManager.gamePhase = GamePhase.GameEnded;
    }
    
    // 🔹 NEW: Handle continuation to next deal
    private void HandleDealsRummyContinuation()
    {
        Debug.Log($"[RoundEndScreen] Deal {gameManager.GetCurrentDealNumber()} completed, continuing to next deal");
        
        FullscreenTextMessage.instance.ShowText(
            $"Deal {gameManager.GetCurrentDealNumber()} Won by {winner.player.userData.username}", 3f);
        
        // Reset players for next deal (but keep cumulative scores)
        foreach (Player player in gameManager.playerList)
        {
            player.ResetForNewDeal();
        }
        
        // Start next deal after a delay
        StartCoroutine(StartNextDealWithDelay());
    }
    
    // 🔹 NEW: Start next deal with delay
    private IEnumerator StartNextDealWithDelay()
    {
        yield return new WaitForSeconds(3f);
        
        Debug.Log("[RoundEndScreen] Starting next deal");
        gameManager.StartNewDeal();
    }
    
    // 🔹 NEW: Determine overall winner in Deals Rummy
    private Player DetermineDealsRummyWinner()
    {
        var players = gameManager.playerList.OrderByDescending(p => p.dealsWon)
                                          .ThenBy(p => p.cumulativeScore)
                                          .ToList();
        
        return players.FirstOrDefault();
    }
    
    // 🔹 NEW: Calculate Deals Rummy winning amount
    private int GetDealsRummyWinningAmount()
    {
        // Calculate based on total bet amount and player count
        double totalPool = gameManager.bid?.totalBet ?? 100;
        int platformFee = gameManager.GetPlatformFee((int)totalPool);
        return (int)(totalPool - platformFee);
    }

    private void HandlePoolRummyEnd(int winnerDeadwoodScore, int loserDeadwoodScore)
    {
        Debug.Log("[RoundEndScreen] Handling Pool Rummy round end");
        
        // Update cumulative scores
        int scoreDifference = loserDeadwoodScore - winnerDeadwoodScore;
        
        // Winner gets 0 points, loser gets the difference
        loser.player.AddToCumulativeScore(scoreDifference);
        
        // 🔹 LIVE FUNCTIONALITY: Check for elimination after score update
        CheckForPlayerElimination();
        
        // 🔹 LIVE FUNCTIONALITY: Check if Pool game should end
        if (gameManager.GetActivePlayersCount() <= 1)
        {
            HandlePoolGameCompletion();
        }
        else
        {
            HandlePoolGameContinuation();
        }
    }
    
    // 🔹 NEW: Check for player elimination in Pool Rummy
    private void CheckForPlayerElimination()
    {
        int eliminationThreshold = gameManager.GetEliminationThreshold();
        
        foreach (Player player in gameManager.playerList)
        {
            if (!player.isEliminated && player.cumulativeScore >= eliminationThreshold)
            {
                Debug.Log($"Player {player.name} eliminated with {player.cumulativeScore} points");
                player.CheckForElimination();
                
                string eliminationInfo = $"{player.userData.username} ELIMINATED! ";
                eliminationInfo += $"(Crossed {eliminationThreshold} points)";
                FullscreenTextMessage.instance.ShowText(eliminationInfo, 4f);
            }
        }
    }
    
    // 🔹 NEW: Handle Pool game completion
    private void HandlePoolGameCompletion()
    {
        Debug.Log("[RoundEndScreen] Pool game completed - only one player remains");
        
        Player poolWinner = gameManager.GetActivePlayers().FirstOrDefault();
        if (poolWinner != null)
        {
            int winningAmount = GetPoolWinningAmount();
            poolWinner.AddScore(winningAmount);
            
            FullscreenTextMessage.instance.ShowText(
                $"{poolWinner.userData.username} Wins Pool!\n₹{winningAmount}", 5f);
            
            Debug.Log($"Pool Rummy completed. Winner: {poolWinner.name}");
        }
        
        // End the game
        gameManager.gamePhase = GamePhase.GameEnded;
    }
    
    // 🔹 NEW: Handle Pool game continuation
    private void HandlePoolGameContinuation()
    {
        Debug.Log("[RoundEndScreen] Pool round completed, game continues");
        
        FullscreenTextMessage.instance.ShowText(
            $"Round Won by {winner.player.userData.username}", 3f);
        
        // Reset for next round (keep cumulative scores)
        foreach (Player player in gameManager.playerList)
        {
            if (!player.hasDropped && !player.isEliminated)
            {
                player.ResetForNewDeal(); // Reset turn-specific state
            }
        }
        
        // Continue to next round after a delay
        StartCoroutine(StartNextRoundWithDelay());
    }
    
    // 🔹 NEW: Start next round with delay
    private IEnumerator StartNextRoundWithDelay()
    {
        yield return new WaitForSeconds(3f);
        
        Debug.Log("[RoundEndScreen] Starting next Pool round");
        // Reset and start new round but keep Pool game state
        gameManager.StartNewMatch();
    }
    
    // 🔹 NEW: Calculate Pool winning amount
    private int GetPoolWinningAmount()
    {
        // Calculate based on total bet amount and player count
        double totalPool = gameManager.bid?.totalBet ?? 100;
        int platformFee = gameManager.GetPlatformFee((int)totalPool);
        return (int)(totalPool - platformFee);
    }
}
