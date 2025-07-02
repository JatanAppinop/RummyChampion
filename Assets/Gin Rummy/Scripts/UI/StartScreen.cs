using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SecPlayerPrefs;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScreen : IHUD
{
	public static StartScreen instance;
	private Button _startBtn;

	protected override void Awake()
	{
		instance = this;
		base.Awake();
		Screen.orientation = ScreenOrientation.LandscapeLeft;
		// _startBtn = GetComponentInChildren<Button>();
		// _startBtn.onClick.AddListener(OnStart);
	}
	private async void Start()
	{
		// GameManager.instance.StartNewGame(GameType.Quick, new Bid(), new GameData());
		TurnOffScreen();

	}

	private async Task getGameData()
	{
		Gamemanager.Instance.MatchID = SecurePlayerPrefs.GetString(Appinop.Constants.KMatchId);
		//Get Match Data
		var matchResponce = await APIServices.Instance.GetAsync<Match>(APIEndpoints.getMatch + Gamemanager.Instance.MatchID);
		if (matchResponce == null && !matchResponce.success)
		{
			UnityNativeToastsHelper.ShowShortText(matchResponce.message);
			return;
		}
		Gamemanager.Instance.MatchData = matchResponce.data;

		//Get Contest 
		Gamemanager.Instance.ContestID = matchResponce.data.tableId;
		Gamemanager.Instance.tableData = DataContext.Instance.contestsData.FirstOrDefault(detail => detail._id == Gamemanager.Instance.ContestID);

	}

	public void OnStart()
	{
		Debug.Log("assasa");
		// GameManager.instance.StartNewGame(GameType.Quick, new Bid(), new GameData());
	}

	public void BackBtnPresed()
	{
		AlertSlider.Instance.Show("Are you sure you want to Quit ?", "Quit", "Cancel").OnPrimaryAction(() =>
		{
			RummySocketServer.Instance.Disconnect();
			Screen.orientation = ScreenOrientation.Portrait;
			SceneManager.LoadScene((int)Scenes.MainMenu);
		});
	}
}
