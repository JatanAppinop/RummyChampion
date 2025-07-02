using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHistoryCell : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI winningLabel;
    [SerializeField] TextMeshProUGUI playersLabel;
    [SerializeField] TextMeshProUGUI entryFeeLabel;
    [SerializeField] TextMeshProUGUI gameStatusLabel;
    [SerializeField] Image gameStatusParent;
    [SerializeField] TextMeshProUGUI playedOnLabel;
    [Header("Configs")]
    [SerializeField] Color winLabelColor;
    [SerializeField] Color lostLabelColor;
    [SerializeField] Color winParentColor;
    [SerializeField] Color lostParentColor;
    [SerializeField] PlayerWin _playerWin;


    public void UpdateData(int players, PlayerWin playerWin)
    {

        _playerWin = playerWin;
        winningLabel.SetText($"₹{playerWin.wonCoin}");
        entryFeeLabel.SetText($"₹{playerWin.bet}");
        playersLabel.SetText($"<b>{players}</b> Players");

        playedOnLabel.SetText($"Played on {playerWin.gameWonDate}");

    }
    public void UpdateData(int players, double wonCoin, double bet, DateTime gameWonDate, bool win = false)
    {

        winningLabel.SetText($"₹{wonCoin}");
        entryFeeLabel.SetText($"₹{bet}");
        playersLabel.SetText($"<b>{players}</b> Players");
        playedOnLabel.SetText($"Played on {gameWonDate}");

        if (win)
        {
            gameStatusLabel.SetText("WON");
            gameStatusLabel.color = winLabelColor;
            gameStatusParent.color = winParentColor;
        }
        else
        {
            gameStatusLabel.SetText("LOSE");
            gameStatusLabel.color = lostLabelColor;
            gameStatusParent.color = lostParentColor;
        }
    }
}
