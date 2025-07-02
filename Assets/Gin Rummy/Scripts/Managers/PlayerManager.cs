using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public static Action<string> OnNicknameChanged;
    public static Action OnMultipassEventGameWon;

    public string playerId;
    private string _playerName;
    public string playerName
    {
        get
        {
            return _playerName;
        }
        set
        {
            _playerName = value;
            OnNicknameChanged.RunAction(value);
        }
    }

    //Account flags
    private bool isPlayerChangeItName;

    private int totalGamePlayed;
    private float totalDeadwood;
    private float bestDeadwood;

    private float totalPoints;
    private float bestPoints;

    private float totalTime;
    private float bestTime;

    private GameManager gameManager;

    private void Awake()
    {
        instance = this;
    }
    
    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void WinGame()
    {
        int prize = gameManager.GetWinPrize();
        CurrencyType currencyType = gameManager.GetGameCurrencyType();
    }

    #region MEDALS_DATA
    public float GetAverageDeadwood()
    {
        if (totalGamePlayed == 0)
            return 0;
        return totalDeadwood / totalGamePlayed;
    }

    public float GetBestDeadwood()
    {
        return bestDeadwood;
    }

    public float GetCurrentDeadwood()
    {
       return gameManager.GetLastGameAverageDeadwood();
    }

    public float GetAveragePoints()
    {
        if (totalGamePlayed == 0)
            return 0;

        return totalPoints/totalGamePlayed;
    }

    public float GetBestPoints()
    {
        return totalPoints;
    }

    public float GetCurrentPoints()
    {
        return gameManager.thisPlayerHand.playerOfThisHand.GetScore();
    }

    public float GetAverageTime()
    {
        if (totalGamePlayed == 0)
            return 0;

        return totalTime/ totalGamePlayed;
    }

    public void SaveGameStatsData(float lastGameAverageDeadwood, int lastGameScore)
    {
        totalGamePlayed++;
        totalDeadwood += lastGameAverageDeadwood;
        totalPoints += lastGameScore;

        if (lastGameAverageDeadwood < bestDeadwood)
            bestDeadwood = lastGameAverageDeadwood;

        if (lastGameScore > bestPoints)
            bestPoints = lastGameScore;
    }

    #endregion
}
