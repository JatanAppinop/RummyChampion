using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[Serializable]
public class Bid
{
    public int id { get; private set; }
    public CurrencyType currencyType { get; private set; }
    public GameType gameType { get; private set; }

    public int entry { get; private set; }
    public int win { get; private set; }
    public int bonusPoints { get; private set; }
    public double totalBet { get; private set; }
    public int pointsToWin { get; private set; }

    public Bid(int id, GameType gameType, CurrencyType currencyType, int entry, int win, int bonusPoints, int pointsToWin)
    {
        this.id = id;
        this.gameType = gameType;
        this.currencyType = currencyType;
        this.entry = entry;
        this.win = win;
        this.bonusPoints = bonusPoints;
        this.pointsToWin = pointsToWin;
    }

    public Bid()
    {
           
    }

    public int GetBalance()
    {
        return entry * 10;
    }

    public int GetAverageExpAmount()
    {
        return Constants.EXP_MULTIPLIER * pointsToWin;
    }
}