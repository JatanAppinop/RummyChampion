using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StoreCoinCell : MonoBehaviour
{
    [SerializeField] List<Sprite> coinSprites;

    [SerializeField] Image coinImage;
    [SerializeField] TextMeshProUGUI coinLabel;
    [SerializeField] TextMeshProUGUI priceLabel;

    [SerializeField] Button button;

    [HideInInspector]
    public UnityEvent<int> onPressed;

    private CoinItem _coinData;

    private void Awake()
    {
        button.onClick.AddListener(onButtonClicked);
    }

    public void UpdateData(CoinItem coinData)
    {
        _coinData = coinData;
        coinImage.sprite = coinSprites[GetSegment(_coinData.coin)];
        coinLabel.SetText($"<b>{_coinData.coin}</b> Coins");
        priceLabel.SetText($"₹{_coinData.coin}");

    }

    private void onButtonClicked()
    {
        onPressed?.Invoke(_coinData.coin);
    }

    public int GetSegment(int coinValue)
    {
        int segment = 0;

        if (coinValue <= 100)
        {
            segment = 0;
        }
        else if (coinValue > 100 && coinValue <= 1000)
        {
            segment = 1;
        }
        else if (coinValue > 1000 && coinValue <= 5000)
        {
            segment = 2;
        }
        else if (coinValue > 5000 && coinValue <= 10000)
        {
            segment = 3;
        }
        else if (coinValue > 10000)
        {
            segment = 4;
        }

        return Math.Min(segment, coinSprites.Count);
    }

}
