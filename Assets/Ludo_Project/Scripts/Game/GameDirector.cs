using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using SecPlayerPrefs;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDirector : SingletonWithoutGameobject<GameDirector>
{
	// Reference


	private BoardColorsUtils.BoardColors playerColor;

	private IEnumerator waitingConnectionRoutine;

	[SerializeField] CanvasGroup loadingScreen;
	[SerializeField] TextMeshProUGUI loadingLabel;

	async void Start()
	{
		loadingScreen.alpha = 1.0f;

		//Get Game Mode
		await GetGameData();
		// Create Players
		CreatePlayers();
		//Setup Player and It's Section
		SetupPlayer();
		//Setup Board
		SetupBoard();
		//Setup Player IDs
		await SetupPlayerIds();
		//Setup Players Tokens
		SetupPlayersTokens();
		//Connect Server
		await ConnectServer();
		//Start Game
		//StartCoroutine(StartGame());


	}

	private async Task GetGameData()
	{
		// Loader.Instance.Show();
		//Fetch Data
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




		Gamemanager.Instance.winningLabel.text = "₹" + Gamemanager.Instance.tableData.wonCoin;

		//Get Color
		string playerColorStr = PlayerPrefs.GetString(Appinop.Constants.KPlayerColorSelect, BoardColorsUtils.BoardColors.Blue.ToString());
		Enum.TryParse(playerColorStr, out BoardColorsUtils.BoardColors playerColor);
		Gamemanager.Instance.selectedColor = playerColor;


		//Get Player Count

		if (Gamemanager.Instance.tableData.gameType == Appinop.Constants.KTwoPlayer)
		{
			Gamemanager.Instance.playerCounts = Appinop.Constants.PlayerCounts.TwoPlayer;
		}
		else if (Gamemanager.Instance.tableData.gameType == Appinop.Constants.KFourPlayer)
		{
			Gamemanager.Instance.playerCounts = Appinop.Constants.PlayerCounts.FourPlayer;
		}
		else
		{
			Gamemanager.Instance.playerCounts = Appinop.Constants.PlayerCounts.TwoPlayer;
		}

		// Get Game Mode
		Enum.TryParse(Gamemanager.Instance.tableData.gameMode, out Appinop.Constants.GameMode gameMode);
		Gamemanager.Instance.gameMode = gameMode;

		switch (gameMode)
		{
			case Appinop.Constants.GameMode.Classic:
				Gamemanager.Instance.gameModeLabel.text = "Classic";
				break;
			case Appinop.Constants.GameMode.Timer:
				Gamemanager.Instance.gameModeLabel.text = "Timer";
				break;
			case Appinop.Constants.GameMode.Turbo:
				Gamemanager.Instance.gameModeLabel.text = "Turbo";
				break;
		}

		//Debug.Log("Finished loading Data");
	}

	private void CreatePlayers()
	{
		//Debug.Log("Creating Players");
		if (Gamemanager.Instance.playerCounts == Appinop.Constants.PlayerCounts.TwoPlayer)
		{
			for (int i = 0; i < 2; i++)
			{
				LudoPlayer player = Gamemanager.Instance.playerControls[i * 2].gameObject.AddComponent<LudoPlayer>();
				player.SetId(i.ToString());
				Gamemanager.Instance.ludoPlayers.Add(player);
				Gamemanager.Instance.onlinePlayers.Add(player);
			}
		}
		else if (Gamemanager.Instance.playerCounts == Appinop.Constants.PlayerCounts.FourPlayer)
		{
			for (int i = 0; i < 4; i++)
			{
				LudoPlayer player = Gamemanager.Instance.playerControls[i].gameObject.AddComponent<LudoPlayer>();
				player.SetId(i.ToString());
				Gamemanager.Instance.ludoPlayers.Add(player);
				Gamemanager.Instance.onlinePlayers.Add(player);
			}

		}
	}

	private void SetupPlayer()
	{
		LudoPlayer player = Gamemanager.Instance.ludoPlayers[0];
		player.userData = UserDataContext.Instance.UserData;
		player.SetId(UserDataContext.Instance.UserData._id);
		player.color = Gamemanager.Instance.selectedColor;
		player.playerType = LudoPlayer.PlayerType.Human;
		player.section = Gamemanager.Instance.board.sections[0];
		player.section.sectionColor = player.color;
		player.section.SetGameMode(Gamemanager.Instance.gameMode);
		Gamemanager.Instance.board.sections[0].UpdateSectionColor(BoardColorsUtils.GetColor(player.color));
		player.playerControls = Gamemanager.Instance.playerControls[0];
		player.playerControls.Initialize(player, true);
		player.UpdatePlayerData();
		player.section.occupied = true;
	}

	private void SetupBoard()
	{
		int numPlayers = (int)Gamemanager.Instance.playerCounts;

		List<BoardColorsUtils.BoardColors> availableColors = new List<BoardColorsUtils.BoardColors> {
		BoardColorsUtils.BoardColors.Red,
		BoardColorsUtils.BoardColors.Green,
		BoardColorsUtils.BoardColors.Blue,
		BoardColorsUtils.BoardColors.Yellow
	};

		availableColors.Remove(Gamemanager.Instance.selectedColor);

		for (int i = 1; i < numPlayers; i++)
		{
			LudoPlayer player = Gamemanager.Instance.ludoPlayers[i];
			player.color = availableColors[0];
			player.section = Gamemanager.Instance.board.sections[i];
			if (numPlayers == 2)
			{
				player.section = Gamemanager.Instance.board.sections[i * 2];
			}
			player.section.sectionColor = player.color;
			player.playerControls = player.GetComponent<PlayerControls>();
			player.playerControls.Initialize(player, false);
			availableColors.RemoveAt(0);
			player.section.occupied = true;
			player.section.UpdateSectionColor(BoardColorsUtils.GetColor(player.section.sectionColor));
			player.section.SetGameMode(Gamemanager.Instance.gameMode);
		}

		foreach (var section in Gamemanager.Instance.board.sections)
		{
			if (!section.occupied)
			{
				BoardColorsUtils.BoardColors color = availableColors[0];
				section.sectionColor = color;
				section.occupied = true;
				section.HidePlayerPlaceholder();
				section.UpdateSectionColor(BoardColorsUtils.GetColor(color));
				availableColors.RemoveAt(0);
			}
		}

		foreach (var controls in Gamemanager.Instance.playerControls)
		{
			if (!controls.GetComponent<LudoPlayer>())
			{
				controls.gameObject.SetActive(false);
			}
		}

		availableColors.Clear();

		//Setup Timer

		if (Gamemanager.Instance.gameMode == Appinop.Constants.GameMode.Timer)
		{
			if (Gamemanager.Instance.playerCounts == Appinop.Constants.PlayerCounts.TwoPlayer)
			{
				Gamemanager.Instance.gameTimer.Initialize(Appinop.Constants.TurboModeTime);
			}
			else if (Gamemanager.Instance.playerCounts == Appinop.Constants.PlayerCounts.FourPlayer)
			{
				Gamemanager.Instance.gameTimer.Initialize(Appinop.Constants.FourPTurboModeTime);
			}
		}

		//setup Dice Drawer
		if (Gamemanager.Instance.gameMode == Appinop.Constants.GameMode.Turbo)
		{
			Gamemanager.Instance.diceDrawerController.Initialize();
			Gamemanager.Instance.diceDrawerController.GenerateNumbers();
			Gamemanager.Instance.movesLeftController.Initialize();
			Gamemanager.Instance.movesLeftController.UpdateMoves(Gamemanager.Instance.diceDrawerController.totalMoves);
		}



	}

	private async Task SetupPlayerIds()
	{

		// Ensure both lists have elements
		if (Gamemanager.Instance.MatchData.players.Count == 0 || Gamemanager.Instance.ludoPlayers.Count == 0)
		{
			Debug.LogWarning("Cannot assign IDs: MatchData.playerIds or ludoPlayers is empty.");
			return;
		}

		// Find the index of your own ID
		int ownIdIndex = Gamemanager.Instance.MatchData.players.FindIndex(id => id == Gamemanager.Instance.ludoPlayers[0].id);

		if (ownIdIndex == -1)
		{
			Debug.LogWarning("Own ID not found in MatchData.playerIds");
			return;
		}

		// Start assigning IDs from the next index after your own
		int nextPlayerIdIndex = (ownIdIndex + 1) % Gamemanager.Instance.MatchData.players.Count;

		for (int i = 1; i < Gamemanager.Instance.ludoPlayers.Count; i++)
		{
			Gamemanager.Instance.ludoPlayers[i].SetId(Gamemanager.Instance.MatchData.players[nextPlayerIdIndex]);
			await Gamemanager.Instance.ludoPlayers[i].GetPlayerData();
			Gamemanager.Instance.ludoPlayers[i].UpdatePlayerData();
			nextPlayerIdIndex = (nextPlayerIdIndex + 1) % Gamemanager.Instance.MatchData.players.Count;

		}
		Gamemanager.Instance.board.Initialize();
	}
	private void SetupPlayersTokens()
	{
		if (Gamemanager.Instance.gameMode == Appinop.Constants.GameMode.Classic)
		{

			foreach (var player in Gamemanager.Instance.ludoPlayers)
			{
				player.CreateTokens();
				player.AnimateToken();
			}
		}
		else if (Gamemanager.Instance.gameMode == Appinop.Constants.GameMode.Timer)
		{
			foreach (var player in Gamemanager.Instance.ludoPlayers)
			{
				player.CreateTokens();

				foreach (var token in player.tokens)
				{
					TokenMovement.Instance.MoveTokenForce(player, token, 1);
				}
			}
		}
		else if (Gamemanager.Instance.gameMode == Appinop.Constants.GameMode.Turbo)
		{
			foreach (var player in Gamemanager.Instance.ludoPlayers)
			{
				player.CreateTokens();

				foreach (var token in player.tokens)
				{
					TokenMovement.Instance.MoveTokenForce(player, token, 1);
				}
			}
		}

	}

	private async Task ConnectServer()
	{
		SocketServer.Instance.Initialize(APIServices.Instance.GetSocketUrl + "/gameserver");
		SocketServer.Instance.onStartMatch.AddListener(onMatchStart);

		loadingLabel.SetText("Connecting Players");

		waitingConnectionRoutine = WaitForPlayersToJoin();
		StartCoroutine(waitingConnectionRoutine);
		await SocketServer.Instance.ConnectServer(Gamemanager.Instance.MatchID);

	}

	IEnumerator WaitForPlayersToJoin()
	{
		int timeout = 60;

		while (timeout > 0)
		{

			if (timeout == (60 - 2))
			{

				SocketServer.Instance.SendEvent("player_ready");
				Debug.Log("Player Ready Event Sent");
			}
			loadingLabel.SetText("Connecting Players\nPlease Wait : " + timeout);

			timeout--;
			yield return new WaitForSeconds(1);
		}

		StartCoroutine(CancelMatchView());

	}

	IEnumerator CancelMatchView()
	{
		Loader.Instance.Hide();
		Gamemanager.Instance.failToStart.SetActive(true);
		Gamemanager.Instance.failToStart.GetComponent<CanvasGroup>().DOFade(1, 0.2f);
		SocketServer.Instance.SendEvent("Match_Cancelled");

		yield return new WaitForSeconds(0.3f);

		int timer = 3;

		while (timer > 0)
		{
			Gamemanager.Instance.failToStartLabel.text = timer.ToString();
			timer--;
			yield return new WaitForSeconds(1.0f);
		}

		SceneManager.LoadScene((int)Scenes.MainMenu);

	}

	private void onMatchStart(string playerId)
	{
		Debug.LogWarning("On Match Start Called");
		SocketServer.Instance.onStartMatch.RemoveListener(onMatchStart);

		StopCoroutine(waitingConnectionRoutine);
		startMatch(playerId);
	}

	private void startMatch(string playerId)
	{
		StartCoroutine(StartGame(playerId));
	}

	IEnumerator StartGame(string playerId)
	{
		yield return new WaitForEndOfFrame();
		loadingLabel.SetText("All Player Connected");
		loadingScreen.DOFade(0, 0.2f).OnComplete(() => loadingScreen.gameObject.SetActive(false));

		Gamemanager.Instance.Initialize(playerId);
	}
}