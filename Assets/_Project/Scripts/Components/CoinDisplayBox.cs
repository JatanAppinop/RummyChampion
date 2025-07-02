using System.Collections;
using System.Collections.Generic;
using Appinop;
using TMPro;
using UnityEngine;

public class CoinDisplayBox : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] bool rupeeSymbol = false;

    private void Awake()
    {
        WalletDataContext.Instance.WalletDataChanged.AddListener(OnWalletDataChanged);
        UpdateWalletData(WalletDataContext.Instance.WalletData);
    }

    private void OnWalletDataChanged(WalletData data)
    {
        UpdateWalletData(data);
    }
    private void UpdateWalletData(WalletData data)
    {
        if (rupeeSymbol)
            label.SetText(data.totalBalance.ToTwoDecimalString() + "₹");
        else
            label.SetText(data.totalBalance.ToTwoDecimalString());
    }
}
