using System;
using System.Collections;
using System.Collections.Generic;
using Appinop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TransactionCell : MonoBehaviour
{
    public enum TransactionType
    {
        Pending,
        Failed,
        Deposit,
        Withdrawal,
        Winnings,
        Fees
    }
    [Header("References")]
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI date;
    [SerializeField] TextMeshProUGUI amount;
    [SerializeField] TextMeshProUGUI status;
    [SerializeField] List<TransactionIcon> icons = new List<TransactionIcon>();

    [Header("Data")]
    [SerializeField] TransactionData data;

    public void UpdateData(TransactionData _data)
    {
        data = _data;
        UpdateUI();
    }

    private void UpdateUI()
    {
        title.SetText(data.title);
        amount.SetText(data.amount.ToTwoDecimalString());
        date.SetText(data.createdAt.ToLocalTime().ToString());
        status.SetText(data.status);
        amount.SetText("+ " + data.amount.ToTwoDecimalString(true));

        switch (data.transactionType.ToLower())
        {
            case "game":
                icon.sprite = icons.Find(i => i.type == TransactionType.Winnings).icon;
                break;
            case "prize":
                icon.sprite = icons.Find(i => i.type == TransactionType.Winnings).icon;
                break;
            case "fee":
                amount.SetText("- " + data.amount.ToTwoDecimalString(true));
                icon.sprite = icons.Find(i => i.type == TransactionType.Withdrawal).icon;
                break;

            case "withdrawal":
                amount.SetText("- " + data.amount.ToTwoDecimalString(true));
                icon.sprite = icons.Find(i => i.type == TransactionType.Withdrawal).icon;
                break;

            case "deposit":
                icon.sprite = icons.Find(i => i.type == TransactionType.Deposit).icon;
                break;
            default:
                amount.SetText(data.amount.ToTwoDecimalString(true));
                icon.sprite = icons.Find(i => i.type == TransactionType.Pending).icon;
                break;
        }

        switch (data.status)
        {
            case "Pending":
                icon.sprite = icons.Find(i => i.type == TransactionType.Pending).icon;
                break;

            case "FAILED":
                icon.sprite = icons.Find(i => i.type == TransactionType.Failed).icon;
                break;

            case "Failed":
                icon.sprite = icons.Find(i => i.type == TransactionType.Failed).icon;
                break;


        }

    }
    [Serializable]
    internal class TransactionIcon
    {
        public TransactionType type;
        public Sprite icon;
    }
}

