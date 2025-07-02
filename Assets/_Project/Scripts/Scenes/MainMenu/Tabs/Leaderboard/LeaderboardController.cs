using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Appinop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardController : PageController
{
    [SerializeField] private bool isLoaded = false;
    [SerializeField] LeaderboardTabGroup SceneTabGroup;
    [SerializeField] TextMeshProUGUI coinLabel;

    [SerializeField] LeaderboardDetailView dailyView;
    [SerializeField] LeaderboardDetailView weeklyView;
    [SerializeField] LeaderboardDetailView monthlyView;



    public override void OnShown()
    {
        if (!isLoaded)
        {
            isLoaded = true;
            WalletDataContext.Instance.WalletDataChanged.AddListener(OnWalletDataChanged);
            UpdateWalletData(WalletDataContext.Instance.WalletData);
        }
        // dailyView.Reset();
        // weeklyView.Reset();
        // monthlyView.Reset();
        UpdateData();
    }

    private void OnWalletDataChanged(WalletData data)
    {
        UpdateWalletData(data);
    }
    private void UpdateWalletData(WalletData data)
    {
        coinLabel.SetText(data.totalBalance.ToTwoDecimalString());
    }

    private async void UpdateData()
    {
        List<LeaderboardItem> leaderboardData = new List<LeaderboardItem>();

        //Get Data
        var responce = await APIServices.Instance.GetAsync<LeaderboardData>(APIEndpoints.leaderboard, includeAuthorization: true);
        if (responce != null && responce.success)
        {
            leaderboardData = responce.data;
        }
        else
        {
            AlertSlider.Instance.Show("Can not load the data at the moment.\nTry again Later.", "OK").OnPrimaryAction(() => AlertSlider.Instance.Hide());
        }

        //Filter Data
        List<LeaderboardItem> filteredData = leaderboardData.Where(item => item.wins > 0).ToList();
        filteredData.Sort((a, b) => b.wins.CompareTo(a.wins));

        //Update Data
        dailyView.UpdateData(filteredData);
        // weeklyView.UpdateData(filteredData);
        // monthlyView.UpdateData(filteredData);


    }
}
