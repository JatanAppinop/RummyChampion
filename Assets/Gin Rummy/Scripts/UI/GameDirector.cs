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

namespace Rummy
{


    public class GameDirector : SingletonWithoutGameobject<GameDirector>
    {

        private IEnumerator waitingConnectionRoutine;

        [SerializeField] CanvasGroup loadingScreen;
        [SerializeField] TextMeshProUGUI loadingLabel;


        async void Start()
        {
            loadingScreen.alpha = 1.0f;
            await GetGameData();
            //Connect Server
            await ConnectServer();

        }
        private async Task GetGameData()
        {
            //Fetch Data
            GameManager.instance.MatchID = SecurePlayerPrefs.GetString(Appinop.Constants.KMatchId);

            //Get Match Data
            var matchResponce = await APIServices.Instance.GetAsync<Match>(APIEndpoints.getMatch + GameManager.instance.MatchID);
            if (matchResponce == null && !matchResponce.success)
            {
                UnityNativeToastsHelper.ShowShortText(matchResponce.message);
                return;
            }
            GameManager.instance.MatchData = matchResponce.data;

            RummySocketServer.Instance.OnDealCard.AddListener(onMatchStart);
            //Get Contest 
            GameManager.instance.ContestID = matchResponce.data.tableId;
            GameManager.instance.tableData = DataContext.Instance.contestsData.FirstOrDefault(detail => detail._id == GameManager.instance.ContestID);

        }

        private void OnDisable()
        {
            RummySocketServer.Instance.OnDealCard.AddListener(onMatchStart);
        }
        private async Task ConnectServer()
        {
            try
            {
                Debug.Log($"🔍 [GAME DIRECTOR] Starting connection to server...");
                Debug.Log($"🔍 [GAME DIRECTOR] Socket URL: {APIServices.Instance.GetSocketUrl}rummyserver");
                Debug.Log($"🔍 [GAME DIRECTOR] Match ID: {GameManager.instance.MatchID}");
                
                RummySocketServer.Instance.Initialize(APIServices.Instance.GetSocketUrl + "rummyserver");

                // 🔧 FIX: Add error event listener before connecting
                RummySocketServer.Instance.OnError.AddListener(HandleConnectionError);

                waitingConnectionRoutine = WaitForPlayersToJoin();
                StartCoroutine(waitingConnectionRoutine);
                
                await RummySocketServer.Instance.ConnectServer(GameManager.instance.MatchID);
                Debug.Log($"✅ [GAME DIRECTOR] Connection established successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ [GAME DIRECTOR] Failed to connect to server: {e.Message}");
                HandleConnectionError($"Connection failed: {e.Message}");
            }
        }

        // 🔧 FIX: Add connection error handler
        private void HandleConnectionError(string errorMessage)
        {
            Debug.LogError($"❌ [GAME DIRECTOR] Connection error: {errorMessage}");
            
            // Stop waiting routine if running
            if (waitingConnectionRoutine != null)
            {
                StopCoroutine(waitingConnectionRoutine);
            }
            
            // Show error to user
            if (loadingLabel != null)
            {
                loadingLabel.SetText($"Connection Error:\n{errorMessage}");
            }
            
            // Start retry mechanism
            StartCoroutine(RetryConnection());
        }

        // 🔧 FIX: Add retry mechanism
        private IEnumerator RetryConnection()
        {
            yield return new WaitForSeconds(2);
            
            Debug.Log($"🔄 [GAME DIRECTOR] Retrying connection...");
            if (loadingLabel != null)
            {
                loadingLabel.SetText("Retrying connection...");
            }
            
            // Retry connection
            _ = ConnectServer();
        }


        private void onMatchStart(PlayerCardsReponce data)
        {
            Debug.LogWarning("On Match Start Called");


            StopCoroutine(waitingConnectionRoutine);
            startMatch(data);
        }

        private void startMatch(PlayerCardsReponce data)
        {
            StartCoroutine(StartGame(data));
        }

        IEnumerator StartGame(PlayerCardsReponce data)
        {
            yield return new WaitForEndOfFrame();
            loadingLabel.SetText("All Player Connected");
            yield return new WaitForSeconds(1);
            loadingScreen.DOFade(0, 0.2f).OnComplete(() => loadingScreen.gameObject.SetActive(false));

            // Bid newBid = new Bid(1001, GameType.Quick, CurrencyType.Chips, 0, 1, 1, 10);
            GameManager.instance.StartNewGame(GameType.Quick, new Bid(), new GameData(), data);

        }

        IEnumerator WaitForPlayersToJoin()
        {
            int timeout = 30;

            while (timeout > 0)
            {

                if (timeout == (30 - 2))
                {
                    // 🔹 FIXED: Send player_ready with proper data
                    SendPlayerReadyWithData();
                    Debug.Log($"[GameDirector] Player Ready event sent with data");
                }

                loadingLabel.SetText("Connecting Players\nPlease Wait : " + timeout);

                timeout--;
                yield return new WaitForSeconds(1);
            }

            StartCoroutine(CancelMatchView());

        }

        IEnumerator CancelMatchView()
        {
            // Gamemanager.Instance.failToStart.SetActive(true);
            // Gamemanager.Instance.failToStart.GetComponent<CanvasGroup>().DOFade(1, 0.2f);
            RummySocketServer.Instance.SendEvent(RummySocketEvents.match_cancelled);

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

        // 🔹 NEW: Send player_ready event with proper data (fixes backend communication)
        private async void SendPlayerReadyWithData()
        {
            try
            {
                Debug.Log("[GameDirector] Sending player_ready event with player data...");
                
                // Create basic player ready data for GameDirector context
                PlayerReadyData playerReadyData = new PlayerReadyData
                {
                    playerId = UserDataContext.Instance.UserData._id,
                    playerName = UserDataContext.Instance.UserData.username,
                    matchId = SecurePlayerPrefs.GetString(Appinop.Constants.KMatchId),
                    gameMode = GameManager.instance?.gameMode.ToString() ?? "Unknown",
                    gameType = GameManager.instance?.tableData?.gameType ?? "Unknown",
                    isReady = true,
                    readyTime = DateTime.UtcNow,
                    playerStatus = "waiting",
                    currentPlayers = 1, // At least this player
                    maxPlayers = 2, // Default assumption
                    tableId = GameManager.instance?.tableData?._id,
                    walletBalance = UserDataContext.Instance.UserData.walletCoins,
                    clientVersion = Application.version,
                    deviceInfo = $"{SystemInfo.deviceModel}_{SystemInfo.operatingSystem}"
                };
                
                Debug.Log($"[GameDirector] Sending player_ready: Player={playerReadyData.playerName}, Match={playerReadyData.matchId}");
                
                // Send enhanced player_ready event
                await RummySocketServer.Instance.SendEnhancedEvent(RummySocketEvents.player_ready, playerReadyData);
                
                Debug.Log("[GameDirector] ✅ player_ready event sent successfully!");
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameDirector] ❌ Failed to send player_ready event: {e.Message}");
                
                // Fallback to basic event
                try
                {
                    var basicData = new Dictionary<string, string>
                    {
                        { "playerId", UserDataContext.Instance.UserData._id },
                        { "matchId", SecurePlayerPrefs.GetString(Appinop.Constants.KMatchId) },
                        { "status", "ready" }
                    };
                    
                    await RummySocketServer.Instance.SendEvent(RummySocketEvents.player_ready, basicData);
                    Debug.Log("[GameDirector] ✅ Fallback player_ready sent!");
                }
                catch (Exception fallbackError)
                {
                    Debug.LogError($"[GameDirector] ❌ Even fallback failed: {fallbackError.Message}");
                }
            }
        }

        private void OnDestroy()
        {

        }
    }
}
