using System.Collections;
using System.Collections.Generic;
using Appinop;
using SecPlayerPrefs;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchMakingPageController : MonoBehaviour
{

    [Header("Labels")]
    [SerializeField] TextMeshProUGUI timerLabel;
    [SerializeField] TextMeshProUGUI waitingLabel;

    private MultiplayerManager mManager;

    private float timeOut = 60;

    private bool matchFound = false;


    public void Show()
    {

        this.gameObject.SetActive(true);
        (this.transform as RectTransform).MoveOutOfScreen(Appinop.RectTransformExtensions.Direction.Left);
        int minutes = Mathf.FloorToInt(timeOut / 60);
        int seconds = Mathf.FloorToInt(timeOut % 60);

        // Update timer label
        timerLabel.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (mManager == null)
        {
            mManager = this.gameObject.AddComponent<MultiplayerManager>();
            mManager.onMatchFound.AddListener(onMatchFound);
        }

        (this.transform as RectTransform).MoveToPosition(Vector3.zero);
        StartCoroutine(findMatch("65fab156b3e2b0cbd3997721"));
        StartCoroutine(startTimer());
    }

    public void onBackButtonPresses()
    {
        (this.transform as RectTransform).MoveOutOfScreen(Appinop.RectTransformExtensions.Direction.Left, true, () => this.gameObject.SetActive(false));
    }

    IEnumerator startTimer()
    {

        float timer = timeOut;

        while (timer > 0 && !matchFound)
        {
            // Calculate minutes and seconds
            int minutes = Mathf.FloorToInt(timer / 60);
            int seconds = Mathf.FloorToInt(timer % 60);

            // Update timer label
            timerLabel.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // Wait for one second
            yield return new WaitForSeconds(1f);

            // Decrease timer
            timer -= 1f;
        }

        if (!matchFound)
        {
            // When timer reaches zero
            timerLabel.text = "00:00";
            matchNotFound();
        }
    }
    private void matchNotFound()
    {
        Debug.Log("Match Not Found");
        mManager.Disconnect();
        Destroy(mManager, 0.1f);
        onBackButtonPresses();
    }
    IEnumerator findMatch(string contestId)
    {
        Debug.Log("Contest ID : " + contestId);
        yield return new WaitForSeconds(1f);
        // mManager.ConnectServer("http://103.175.163.162:5093", contestId);

    }

    private void onMatchFound(string matchId)
    {
        Debug.Log("Match Found : " + matchId);
        matchFound = true;
        LoadMatchData(matchId);
    }

    private async void LoadMatchData(string matchId)
    {
        Loader.Instance.Show();

        var matchResponce = await APIServices.Instance.GetAsync<Match>(APIEndpoints.getMatch + matchId, includeAuthorization: false);
        if (matchResponce == null && !matchResponce.success)
        {
            Loader.Instance.Hide();
            return;
        }

        // Save Match ID
        SecurePlayerPrefs.SetString(Appinop.Constants.KMatchId, matchId);
        mManager.Disconnect();
        StartCoroutine(startGame());

        Loader.Instance.Hide();
    }

    IEnumerator startGame()
    {
        int i = 3;
        while (i > 0)
        {
            waitingLabel.text = "Starting Game in " + i;
            yield return new WaitForSeconds(1f);
            i--;
        }

        SceneManager.LoadScene("GameScene");
    }

}
