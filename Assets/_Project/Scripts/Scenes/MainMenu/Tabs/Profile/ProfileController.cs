using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Appinop;
using TMPro;
using UnityEngine;

public class ProfileController : PageController
{
    [SerializeField] private bool isLoaded = false;

    [SerializeField] ProfilePhoto profilePhoto;
    [SerializeField] TextMeshProUGUI number;

    [SerializeField] TextMeshProUGUI totalEarning;
    [SerializeField] TextMeshProUGUI gameWon;
    [SerializeField] TextMeshProUGUI winRate;
    [SerializeField] TextMeshProUGUI totalGamesPlayed;
    [SerializeField] TextMeshProUGUI ludo2PGamesPlayed;
    [SerializeField] TextMeshProUGUI ludo4PGamesPlayed;
    [SerializeField] List<PlayerWin> Player2WinData;
    [SerializeField] List<PlayerWin> Player4WinData;
    [SerializeField] List<PlayerWin> player2RummyData;
    [SerializeField] List<PlayerWin> player6RummyData;


    public override void OnShown()
    {
        if (!isLoaded)
        {
            isLoaded = true;
            UserDataContext.Instance.ProfileDataChanged.AddListener(OnProfileDataChanged);
            UpdateProfile(UserDataContext.Instance.profileData);

            totalEarning.SetText("...");
            gameWon.SetText("...");
            winRate.SetText("...");
            totalGamesPlayed.SetText("...");
            ludo2PGamesPlayed.SetText("...");
            ludo4PGamesPlayed.SetText("...");
        }

        FetchData();

    }

    public async void FetchData()
    {

        var responce = await APIServices.Instance.GetAsync<UserProfile>(APIEndpoints.playerProfile, includeAuthorization: true);
        if (responce != null && responce.success)
        {
            totalEarning.SetText("₹" + responce.data.totalEarnings.ToTwoDecimalString());
            gameWon.SetText(responce.data.gamesWon.ToString());
            winRate.SetText($"{responce.data.winRate.ToInt()} %");
            totalGamesPlayed.SetText(responce.data.totalGamesPlayed.ToString());
            int twoPWin = responce.data.player2Wins != null ? responce.data.player2Wins.Count : 0;
            int fourPWin = responce.data.player4Wins != null ? responce.data.player4Wins.Count : 0;
            int twoRummyPWin = responce.data.player2Rummy != null ? responce.data.player2Rummy.Count : 0;
            int fourRummyPWin = responce.data.player6Rummy != null ? responce.data.player6Rummy.Count : 0;

            ludo2PGamesPlayed.SetText((twoPWin + fourPWin).ToString());
            ludo4PGamesPlayed.SetText((twoRummyPWin + fourRummyPWin).ToString());

            Player2WinData = responce.data.player2Wins?.OrderByDescending(data => data.gameWonDate).ToList();
            Player4WinData = responce.data.player4Wins?.OrderByDescending(data => data.gameWonDate).ToList();
            player2RummyData = responce.data.player2Rummy?.OrderByDescending(data => data.gameWonDate).ToList();
            player6RummyData = responce.data.player6Rummy?.OrderByDescending(data => data.gameWonDate).ToList();
        }
        else
        {
            Debug.LogError("Unable to Update Profile Data");
            UnityNativeToastsHelper.ShowShortText("Something went Wrong.");
        }
    }

    private void OnProfileDataChanged(ProfileData data)
    {
        UpdateProfile(data);
    }


    private void UpdateProfile(ProfileData data)
    {
        profilePhoto.UpdateData(data);
        number.SetText(Extentions.MaskMobile(UserDataContext.Instance.profileData.phNo));
    }


    public void on2PlayerWinsClicked()
    {
        PopoverViewController.Instance.Show(PopoverViewController.Instance.gameHistoryPopover,
        new KeyValuePair<string, object>("KData", Player2WinData),
        new KeyValuePair<string, object>("KTitle", "2 Players Wins"),
        new KeyValuePair<string, object>("KPlayers", 2)

        );
    }
    public void on4PlayerWinsClicked()
    {
        PopoverViewController.Instance.Show(PopoverViewController.Instance.gameHistoryPopover,
        new KeyValuePair<string, object>("KData", Player4WinData),
        new KeyValuePair<string, object>("KTitle", "4 Players Wins"),
        new KeyValuePair<string, object>("KPlayers", 4)
        );
    }
    public void onLudoGamesClicked()
    {
        var joinedList = new List<PlayerWin>();
        joinedList.AddRange(Player2WinData);
        joinedList.AddRange(Player4WinData);

        PopoverViewController.Instance.Show(PopoverViewController.Instance.gameHistoryPopover,
        new KeyValuePair<string, object>("KData", joinedList),
        new KeyValuePair<string, object>("KTitle", "Ludo Games"),
        new KeyValuePair<string, object>("KGame", "ludo")

        );
    }
    public void onRummyGamesClicked()
    {
        var joinedList = new List<PlayerWin>();
        joinedList.AddRange(player2RummyData);
        joinedList.AddRange(player6RummyData);

        PopoverViewController.Instance.Show(PopoverViewController.Instance.gameHistoryPopover,
        new KeyValuePair<string, object>("KData", joinedList),
        new KeyValuePair<string, object>("KTitle", "Rummy Games"),
        new KeyValuePair<string, object>("KGame", "rummy")
        );
    }

}
