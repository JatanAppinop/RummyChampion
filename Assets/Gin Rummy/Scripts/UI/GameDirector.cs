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
            RummySocketServer.Instance.Initialize(APIServices.Instance.GetSocketUrl + "/rummyserver");

            waitingConnectionRoutine = WaitForPlayersToJoin();
            StartCoroutine(waitingConnectionRoutine);
            await RummySocketServer.Instance.ConnectServer(GameManager.instance.MatchID);

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
                    RummySocketServer.Instance.SendEvent(RummySocketEvents.player_ready);
                    Debug.LogError($"Player Ready");
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
    }
}
