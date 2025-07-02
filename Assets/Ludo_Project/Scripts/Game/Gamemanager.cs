using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetKits.ParticleImage;
using DG.Tweening;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Gamemanager : SingletonWithoutGameobject<Gamemanager>
{

    [field: SerializeField] public bool DevMode { get; private set; } = false;

    [Space(20)]
    public TextMeshProUGUI gameModeLabel;
    public bool ReadyToPlay = false;
    public List<LudoPlayer> ludoPlayers;
    public List<LudoPlayer> onlinePlayers;
    public List<LudoPlayer> disconnectedPlayers;
    public LudoBoard board;
    public List<PlayerControls> playerControls;
    public TextMeshProUGUI winningLabel;
    public GameObject playerDisconnected;
    public TextMeshProUGUI disconnectionLabel;
    public GameObject failToStart;
    public TextMeshProUGUI failToStartLabel;

    public DiceDrawerController diceDrawerController;
    public EmojiReactionManager emojiReactionManager;

    // Multiplayer
    public string MatchID;
    public MatchData MatchData;
    public string ContestID;
    public TableData tableData;
    public BoardColorsUtils.BoardColors selectedColor;

    public Appinop.Constants.PlayerCounts playerCounts;
    public Appinop.Constants.GameMode gameMode;
    public GameTimer gameTimer;
    public MovesLeftController movesLeftController;
    [SerializeField] ToggleButton musicToggleBtn;
    [SerializeField] ToggleButton soundToggleBtn;
    [SerializeField] ParticleImage gameFinished;



    private void Start()
    {
        musicToggleBtn.updateButton(!AudioManager.Instance.IsMusicMuted());
        soundToggleBtn.updateButton(!AudioManager.Instance.IsSFXMuted());
    }


    public void Initialize(string playerId)
    {

        SocketServer.Instance.onNextTurn.AddListener(NextTurn);
        SocketServer.Instance.onPlayerDisconnected.AddListener(PlayerDiconnected);
        SocketServer.Instance.onGameFinished.AddListener(GameFinished);
        SocketServer.Instance.onTurnMissed.AddListener(TurnMissed);

        if (gameMode == Appinop.Constants.GameMode.Timer)
        {
            gameTimer.onTimerFinished.AddListener(OnTimerRunout);
            gameTimer.StartTimer();
        }

        Debug.Log("First Turn : " + playerId);
        startGame(playerId);
        AudioManager.Instance.PlayMusic("BG");

        emojiReactionManager.Initialize();

    }

    private void GameFinished(Dictionary<string, string> arg0)
    {
        Debug.Log("Game_Finished event called :- "+ arg0);
        foreach (LudoPlayer p in ludoPlayers)
        {
            UpdatePoints();
            p.FinishPlayerTurn();
        }

        gameTimer.StopTimer();

        StartCoroutine(ChangeToGameOverScene());

    }



    private void startGame(string playerId)
    {
        ludoPlayers.Find(p => p.id == playerId).StartPlayerTurn();
    }
    private void Awake()
    {
        board = FindObjectOfType<LudoBoard>();
    }

    public async void TurnFinished(LudoPlayer lastPlayer)
    {
        if (gameMode == Appinop.Constants.GameMode.Turbo && lastPlayer.playerControls.hasAuthority)
        {
            // ludoPlayers.ForEach(p => p.UpdatePlayerData());
            movesLeftController.UpdateMoves(diceDrawerController.GetRemainingMoves());
        }
        await CheckForGameover();


        Dictionary<string, string> data = new Dictionary<string, string>() {
            { "playerId", lastPlayer.id },
             };
        await SocketServer.Instance.SendEvent("Turn_Finished2", data);
        Debug.Log("Turn Finish Even Sent");


    }

    private async Task CheckForGameover()
    {
        foreach (var player in ludoPlayers)
        {

            if (player.Mistries == 3)
            {
                Dictionary<string, string> data = new Dictionary<string, string>() {
                { "playerId", player.id },
                { "reason", "misstry" },
                };
                await SocketServer.Instance.SendEvent("Game_Finished", data);
                // SceneManager.LoadScene((int)Scenes.GameOver);
            }

            if (player.tokens.Count == 0)
            {
                Dictionary<string, string> data = new Dictionary<string, string>() {
                { "playerId", player.id },
                { "reason", "won" },
                };
                await SocketServer.Instance.SendEvent("Game_Finished", data);
                // SceneManager.LoadScene((int)Scenes.GameOver);
            }

        }

        if (gameMode == Appinop.Constants.GameMode.Turbo)
        {

            bool isMovesOver = true;
            foreach (var player in ludoPlayers)
            {
                if (player.moves < 30)
                {
                    isMovesOver = false; // If any player has made fewer than 30 moves, set to false
                    break;
                }
            }

            if (isMovesOver)
            {

                OnTimerRunout();
            }


        }

    }

    public void UpdatePoints()
    {
        if (gameMode == Appinop.Constants.GameMode.Turbo || gameMode == Appinop.Constants.GameMode.Timer)
        {
            ludoPlayers.ForEach(p =>
            {
                p.CalculatePoints();
                p.section.UpdatePointsLabels(p.points);
                Debug.Log("point updated");
            });
        }
    }

    private void NextTurn(string lastPlayerId)
    {
        Debug.Log("next Turn");

        // UpdatePoints();


        LudoPlayer lastPlayer = ludoPlayers.Find(p => p.id == lastPlayerId);


        StartCoroutine(startNextTurn(lastPlayer));


    }

    private async void OnTimerRunout()
    {
        Debug.LogError("Timer Run Out");
        foreach (LudoPlayer p in ludoPlayers)
        {
            p.FinishPlayerTurn();
        }


        LudoPlayer player = ludoPlayers.OrderByDescending(player => player.points).FirstOrDefault();

        if (gameMode == Appinop.Constants.GameMode.Turbo)
        {
            int bonusPoint = 0;

            player = ludoPlayers.OrderByDescending(player => player.points + ((30 - player.moves) * 5)).FirstOrDefault();
        }

        Dictionary<string, string> data = new Dictionary<string, string>() {
                { "playerId", player.id },
                { "reason", "time_runout" },
                };
        Debug.LogWarning($"Winner is {player.id} , {player.userData.mobileNumber}");
        await SocketServer.Instance.SendEvent("Game_Finished", data);

    }
    private IEnumerator ChangeToGameOverScene()
    {
        // Loader.Instance.Show("Finishing the Game");
        gameFinished.Play();
        yield return new WaitForSeconds(2);
        // Loader.Instance.Hide();
        Debug.Log($"Changing to Winning Scene");
        SceneManager.LoadScene((int)Scenes.GameOver);
    }
    private void PlayerDiconnected(string playerId)
    {

        if (MatchData.players.Contains(playerId) && playerId != UserDataContext.Instance.UserData._id)
        {
            if (playerCounts == Appinop.Constants.PlayerCounts.TwoPlayer)
            {
                Debug.Log("Player Disconnected.");
                StartCoroutine(playerDisconnectedView());
            }
            if (playerCounts == Appinop.Constants.PlayerCounts.FourPlayer)
            {
                Debug.Log("Player Disconnected.");

                LudoPlayer disOnlinePlayer = onlinePlayers.Find(p => p.id == playerId);
                onlinePlayers.Remove(disOnlinePlayer);
                disconnectedPlayers.Add(disOnlinePlayer);

                LudoPlayer disPlayer = ludoPlayers.Find(p => p.id == playerId);
                disPlayer.playerControls.EndTurn();

                if (disPlayer.currentState == LudoPlayer.LudoPlayerState.Turn)
                {
                    disPlayer.FinishPlayerTurn();
                    Gamemanager.Instance.TurnFinished(disPlayer);

                }


                if (onlinePlayers.Count() == 1)
                {
                    StartCoroutine(playerDisconnectedView());
                }
            }

        }
    }

    private void TurnMissed(string playerId)
    {
        Debug.LogWarning("Turn Missed : " + playerId);
        LudoPlayer player = ludoPlayers.Find(p => p.id == playerId);

        if (!player.playerControls.hasAuthority)
        {
            player.playerControls.ReduceMisstry();
        }

    }

    IEnumerator playerDisconnectedView()
    {
        playerDisconnected.SetActive(true);
        playerDisconnected.GetComponent<CanvasGroup>().DOFade(1, 0.2f);

        foreach (LudoPlayer player in ludoPlayers)
        {
            player.FinishPlayerTurn();
        }

        yield return new WaitForSeconds(0.3f);

        int timer = 3;

        while (timer > 0)
        {
            disconnectionLabel.text = timer.ToString();
            timer--;
            yield return new WaitForSeconds(1.0f);
        }

        playerDisconnected.GetComponent<CanvasGroup>().DOFade(0, 0.2f).OnComplete(() => playerDisconnected.SetActive(false));

        SceneManager.LoadScene((int)Scenes.GameOver);
    }

    IEnumerator startNextTurn(LudoPlayer lastPlayer)
    {
        if (gameMode == Appinop.Constants.GameMode.Turbo || gameMode == Appinop.Constants.GameMode.Timer)
        {
            lastPlayer.UpdatePoints();

        }

        if (!lastPlayer.playerControls.ExtraTurn)
        {
            while (lastPlayer.currentState != LudoPlayer.LudoPlayerState.Waiting)
            {
                Debug.Log("Waiting for Turn to Finish for " + lastPlayer.name);
                yield return new WaitForSeconds(0.2f); // Wait for the next frame
            }
        }

        ludoPlayers.ForEach(player =>
        {
            player.playerControls.StopTimer();
            // player.playerControls.DeactivateTokens();
            player.FinishPlayerTurn();
        });


        ExecuteNextTurn(lastPlayer);

    }

    private void ExecuteNextTurn(LudoPlayer lastPlayer)
    {

        if (onlinePlayers.Count > 1)
        {

            bool isLastPlayerActive = onlinePlayers.Find(p => p.id == lastPlayer.id) != null;

            Debug.Log("Executing Next Turn");
            if (lastPlayer.playerControls.ExtraTurn && isLastPlayerActive)
            {
                lastPlayer.playerControls.ExtraTurn = false;
                AudioManager.Instance.PlayEffect("Extra_Roll");
                Debug.Log("Extra Turn");
                lastPlayer.StartPlayerTurn();
            }
            else
            {

                int lastPlayerIndex = ludoPlayers.IndexOf(lastPlayer);

                if (lastPlayerIndex == -1)
                {
                    Debug.LogError("Error: Last player not found in the list");
                }
                else
                {
                    int nextPlayerIndex = (lastPlayerIndex + 1) % ludoPlayers.Count;

                    var onlineNextPlayer = onlinePlayers.Find(p => p.id == ludoPlayers[nextPlayerIndex].id);

                    while (onlineNextPlayer == null)
                    {

                        nextPlayerIndex = (nextPlayerIndex + 1) % ludoPlayers.Count;
                        onlineNextPlayer = onlinePlayers.Find(p => p.id == ludoPlayers[nextPlayerIndex].id);
                    }

                    ludoPlayers[nextPlayerIndex].StartPlayerTurn();


                }
            }
        }
        else
        {
            Debug.LogError("Online Players not Found , " + onlinePlayers.Count());
        }
    }

    public void BackButtonPressed()
    {
        AlertSlider.Instance.Show("Are you sure you want to quit the game? You will Loose the Joining Amount.", "Quit", "Cancel")
        .OnPrimaryAction(() =>
        {
            SocketServer.Instance.Disconnect();
            if (playerCounts == Appinop.Constants.PlayerCounts.TwoPlayer)
            {

                SceneManager.LoadScene((int)Scenes.GameOver);
            }
            else
            {
                SceneManager.LoadScene((int)Scenes.MainMenu);

            }
        });
    }

    public void onMusicToggleClicked()
    {
        AudioManager.Instance.ToggleMusic();
    }
    public void onSoundToggleClicked()
    {
        AudioManager.Instance.ToggleSFX();
    }



    IEnumerator TimeoutTimerRoutine()
    {

        float timer = 0f;
        while (timer < Appinop.Constants.TurnTimeout)
        {
            yield return new WaitForSeconds(1f);
            timer++;
        }
        Debug.LogError("Timer Run out for timeout");
    }
}