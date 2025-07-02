using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
            // Deals Rummy: Winner gets total entry pool after rake deduction
            winningAmount = gameManager.GetDealsWinningAmount();
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

        }
        else if (gameManager.tableData.gameMode.Contains("Pool"))
        {
            //// Pool Rummy: Winner gets total pool if match is over
            //if (CheckIfMatchIsOver())
            //{
            //    winningAmount = gameManager.GetPoolWinningAmount();
            //}

            RummyGameResults(winnerDeadwoodScore, loserDeadwoodScore);
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
}
