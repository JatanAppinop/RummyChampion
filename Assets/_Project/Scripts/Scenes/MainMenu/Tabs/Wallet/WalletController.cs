using System.Collections;
using System.Collections.Generic;
using Appinop;
using TMPro;
using UnityEngine;

public class WalletController : PageController
{
    [SerializeField] private bool isLoaded = false;

    [SerializeField] TextMeshProUGUI totalBalance;
    [SerializeField] TextMeshProUGUI deposits;
    [SerializeField] TextMeshProUGUI winning;
    [SerializeField] TextMeshProUGUI cashback;
    [SerializeField] TextMeshProUGUI bonus;

    [SerializeField] WalletData _walletData;

    private string inrIcon = "₹";
    public override void OnShown()
    {
        if (!isLoaded)
        {
            isLoaded = true;
            totalBalance.text = inrIcon + " ...";
            deposits.text = inrIcon + " ...";
            winning.text = inrIcon + " ...";
            cashback.text = inrIcon + " ...";
            bonus.text = inrIcon + " ...";
            WalletDataContext.Instance.WalletDataChanged.AddListener(OnWalletDataChanged);
            UpdateWalletData(WalletDataContext.Instance.WalletData);
        }
        WalletDataContext.Instance.RefreshData();
    }


    private void OnWalletDataChanged(WalletData data)
    {
        UpdateWalletData(data);
    }
    private void UpdateWalletData(WalletData data)
    {
        totalBalance.text = inrIcon + " " + data.totalBalance.ToTwoDecimals();
        deposits.text = inrIcon + " " + data.depositBalance.ToTwoDecimals();
        winning.text = inrIcon + " " + data.winningBalance.ToTwoDecimals();
        cashback.text = inrIcon + " " + data.cashBonus.ToTwoDecimals();
        bonus.text = inrIcon + " " + data.bonus.ToTwoDecimals();
    }

}
