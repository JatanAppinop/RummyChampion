using System;
using System.Collections;
using System.Collections.Generic;
using Appinop;
using Appinop.PhonePePlugin;
using TMPro;
using UnityEngine;

public class StoreController : PageController
{
    [SerializeField] private bool isLoaded = false;
    [SerializeField] List<StoreCoinCell> storeCoinCells;
    [SerializeField] AdvancedInputField customAmountField;

    private List<CoinItem> coins;
    public void onShown()
    {
    }

    public override void OnShown()
    {
        if (!isLoaded)
        {
            storeCoinCells.ForEach(coin => coin.gameObject.SetActive(false));
            FetchData(isLoaded);
            isLoaded = true;
            PhonePeHelper.Instance.InitializePhonePe(PhonePeHelper.ENVIRONMENT.PRODUCTION, "RUMMYCHAMONLINE", "com.appinop.rummychampions.open");

        }
        FetchData(isLoaded);

    }


    public async void FetchData(bool loaded)
    {

        if (!loaded)
            Loader.Instance.Show();

        var response = await APIServices.Instance.GetAsync<CoinList>(APIEndpoints.getCoins);

        if (response != null && response.success)
        {
            coins = response.data;
            List<CoinItem> activeCoins = coins.FindAll(coin => coin.isActive);
            activeCoins.Sort((a, b) => a.coin.CompareTo(b.coin));
            coins = activeCoins;



            UpdateList(activeCoins);
        }
        else
        {
            Debug.LogError("Error Fetching Coins List");
            UnityNativeToastsHelper.ShowShortText("Something Went Wrong");
        }


        if (!loaded)
            Loader.Instance.Hide();

        customAmountField.HideError();
        customAmountField.text = "";

    }

    private void UpdateList(List<CoinItem> coins)
    {
        if (coins != null && coins.Count > 0)
        {

            for (int i = 0; i < coins.Count; i++)
            {
                CoinItem coinItem = coins[i];
                StoreCoinCell cell = storeCoinCells[i];
                cell.onPressed.RemoveAllListeners();
                cell.UpdateData(coinItem);
                cell.onPressed.AddListener(onCoinPurchaseButtonClicked);
                cell.gameObject.SetActive(true);
            }

        }

    }
    public void onCoinPurchaseButtonClicked(int value)
    {

        PopoverViewController.Instance.Show(PopoverViewController.Instance.depositCalc, new KeyValuePair<string, object>(Appinop.Constants.KDepositAmount, value));
    }

    public void onCustomDepositPressed()
    {
        if (int.TryParse(customAmountField.text, out int amount))
        {
            if (amount >= coins[0].coin)
            {

                PopoverViewController.Instance.Show(PopoverViewController.Instance.depositCalc, new KeyValuePair<string, object>(Appinop.Constants.KDepositAmount, amount));

            }
            else
            {
                customAmountField.ShowError("Minimum Deposit ₹" + coins[0].coin);

            }
        }
        else
        {
            customAmountField.ShowError("Enter Valid Amount");
        }
    }
}
