using System.Collections;
using System.Collections.Generic;
using Appinop;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LudoContestButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI winLabel;
    [SerializeField] TextMeshProUGUI entrytitleLabel;
    [SerializeField] TextMeshProUGUI entryLabel;
    [SerializeField] TextMeshProUGUI onlinePlayerLabel;
    [SerializeField] TextMeshProUGUI totalPlayersLabel;
    [SerializeField] RectTransform winningLabel;
    [SerializeField] RectTransform dealGO;
    [SerializeField] TextMeshProUGUI dealTableLabel;
    [SerializeField] Button button;
    [HideInInspector] public UnityEvent<LudoContestButton> onPressed;

    public TableData data;
    private void onClick() => onPressed.Invoke(this);

    private void Awake()
    {
        button.onClick.AddListener(onClick);
    }

    public void UpdateOnlinePlayerCount(int players)
    {

        if (onlinePlayerLabel) onlinePlayerLabel.text = players + " Online";
    }
    public void UpdateData(TableData _data)
    {
        this.data = _data;

        if (winLabel) winLabel.text = _data.wonCoin.ToTwoDecimalString();
        if (entryLabel) entryLabel.text = "₹" + _data.bet.ToString();
        if (totalPlayersLabel) totalPlayersLabel.text = _data.gameType.Substring(0, 1) + " Players";

        if (dealGO) dealGO.gameObject.SetActive(false);
        if (winningLabel) winningLabel.gameObject.SetActive(true);

        if (_data.game.ToLower() == "rummy" && _data.bet > 0)
        {
            if (_data.gameMode.ToLower() == "points")
            {
                if (entrytitleLabel) entrytitleLabel.SetText("Point Value");
                if (winLabel) winLabel.text = "₹" + _data.pointValue.ToString();
            }
            else if (_data.gameMode.ToLower() == "deals")
            {
                if (dealGO) dealGO.gameObject.SetActive(true);
                if (winningLabel) winningLabel.gameObject.SetActive(false);
                dealTableLabel.SetText("2 Deals");
            }
            else if (_data.gameMode.ToLower() == "101 pool")
            {
                if (dealGO) dealGO.gameObject.SetActive(true);
                if (winningLabel) winningLabel.gameObject.SetActive(false);
                dealTableLabel.SetText("101");
            }
            else if (_data.gameMode.ToLower() == "201 pool")
            {
                if (dealGO) dealGO.gameObject.SetActive(true);
                if (winningLabel) winningLabel.gameObject.SetActive(false);
                dealTableLabel.SetText("201");
            }
        }
        else
        {
            if (entrytitleLabel) entrytitleLabel.SetText("Winning");
        }

    }
    private void OnDestroy()
    {
        button.onClick.RemoveListener(onClick);
    }
}
