using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Appinop;
using AssetKits.ParticleImage;
using DG.Tweening;
using SecPlayerPrefs;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameoverViewController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<GameoverListCell> players;
    [SerializeField] private RectTransform spinner;
    [SerializeField] private CanvasGroup view;
    [SerializeField] private CanvasGroup backButton;
    [Header("Win References")]
    [SerializeField] GameObject WinParent;
    [SerializeField] private Image WinImage;
    [SerializeField] private Image TrumpetLeft;
    [SerializeField] private Image TrumpetRight;
    [SerializeField] private List<ParticleImage> particles;
    [SerializeField] private ParticleImage coinParticle;


    [Header("Loose References")]
    [SerializeField] GameObject LooseParent;
    [SerializeField] private Image LooseImage;
    private bool isLoaded = false;
    private string matchId;
    private MatchData matchData;
    private TableData contestDetail;
    // private Appinop.Constants.PlayerCounts playerCounts;
    [SerializeField] private List<UserData> playerData = new List<UserData>();

    private Vector3 LeftTrumpetScale, RightTrumpetScale;


    private void Awake()
    {
        //view.alpha = 0;
        spinner.gameObject.SetActive(true);
        matchId = SecurePlayerPrefs.GetString(Appinop.Constants.KMatchId);

        backButton.alpha = 0;

        WinParent.SetActive(false);
        WinImage.color = Color.clear;
        WinImage.gameObject.SetActive(false);

        TrumpetLeft.gameObject.SetActive(false);
        TrumpetRight.gameObject.SetActive(false);

        TrumpetLeft.color = Color.clear;
        TrumpetRight.color = Color.clear;

        // LeftTrumpetScale = TrumpetLeft.rectTransform.localScale;
        // RightTrumpetScale = TrumpetRight.rectTransform.localScale;

        // TrumpetLeft.rectTransform.localScale = Vector2.zero;
        // TrumpetRight.rectTransform.localScale = Vector2.zero;

        TrumpetLeft.rectTransform.Rotate(new Vector3(0, 0, 110));
        TrumpetRight.rectTransform.Rotate(new Vector3(0, 0, -110));

        LooseParent.SetActive(false);
        LooseImage.color = Color.clear;
        LooseImage.gameObject.SetActive(false);

        players.ForEach(p => p.gameObject.SetActive(false));
        players.ForEach(p => p.GetComponent<CanvasGroup>().alpha = 0);

    }

    private async void Start()
    {
        UserDataContext.Instance.RefreshData();
        await GetGameData();
        UpdateData();
        ShowAnimation();

        AudioManager.Instance.PlayMusic("MainMenu");

        UserDataContext.Instance.RefreshData();
    }

    private async Task GetGameData()
    {


        var matchResponce = await APIServices.Instance.GetAsync<Match>(APIEndpoints.getMatch + matchId);
        if (matchResponce == null && !matchResponce.success)
        {
            UnityNativeToastsHelper.ShowShortText(matchResponce.message);
            SceneManager.LoadScene((int)Scenes.MainMenu);
            return;
        }
        matchData = matchResponce.data;

        contestDetail = DataContext.Instance.contestsData.FirstOrDefault(detail => detail._id == matchData.tableId);




        //Get Player Data

        foreach (string playerId in matchData.winner)
        {

            if (playerId == UserDataContext.Instance.UserData._id)
            {
                playerData.Add(UserDataContext.Instance.UserData);
                Debug.Log("Default User Data Found");

            }
            else
            {


                var responce = await APIServices.Instance.GetAsync<User>(APIEndpoints.getUser + playerId);

                if (responce != null && responce.success)
                {
                    playerData.Add(responce.data);
                }
                else
                {
                    UnityNativeToastsHelper.ShowShortText(responce.message);
                }
            }
        }
        Debug.Log("Finished loading Data");

    }

    private void UpdateData()
    {
        for (int i = 0; i < playerData.Count; i++)
        {
            players[i].UpdateData(playerData[i]);
            players[i].UpdateRank(i + 1);

            if (i == 0)
                players[i].UpdateWinAmt(contestDetail.wonCoin.ToTwoDecimals());
            else
                players[i].UpdateWinAmt(-contestDetail.bet);
        }
    }

    private void ShowAnimation()
    {
        HideSpinner();

        if (playerData[0]._id == UserDataContext.Instance.UserData._id)
            StartCoroutine(ShowWinAnimation());
        else
            StartCoroutine(ShowLooseAnimation());
    }

    IEnumerator ShowWinAnimation()
    {
        yield return null;
        WinParent.SetActive(true);
        view.DOFade(1, 0.2f);

        WinImage.gameObject.SetActive(true);
        RectTransform winImageTransform = WinImage.transform as RectTransform;
        Vector2 winImagePos = winImageTransform.anchoredPosition;
        winImageTransform.anchoredPosition = new Vector2(winImagePos.x, winImagePos.y - 200f);
        WinImage.DOColor(Color.white, 0.4f);
        winImageTransform.DOAnchorPosY(winImagePos.y, 0.4f);

        yield return new WaitForSeconds(0.4f);

        TrumpetLeft.gameObject.SetActive(true);
        RectTransform trumpetLeftRect = TrumpetLeft.transform as RectTransform;
        TrumpetLeft.DOColor(Color.white, 0.5f);
        // trumpetLeftRect.DOScale(LeftTrumpetScale, 0.6f);
        trumpetLeftRect.DORotate(Vector3.zero, 0.5f).SetEase(Ease.OutBounce);

        TrumpetRight.gameObject.SetActive(true);
        RectTransform trumpeRightRect = TrumpetRight.transform as RectTransform;
        TrumpetRight.DOColor(Color.white, 0.5f);
        // trumpeRightRect.DOScale(RightTrumpetScale, 0.6f);
        trumpeRightRect.DORotate(Vector3.zero, 0.5f).SetEase(Ease.OutBounce);

        yield return new WaitForSeconds(0.5f);

        particles.ForEach(p => p.Play());

        for (int i = 0; i < playerData.Count(); i++)
        {
            players[i].gameObject.SetActive(true);
            players[i].GetComponent<CanvasGroup>().DOFade(1, 0.4f);
            yield return new WaitForSeconds(0.6f);
        }

        coinParticle.Play();

        yield return new WaitForSeconds(0.5f);
        backButton.DOFade(1, 0.4f);

    }

    IEnumerator ShowLooseAnimation()
    {
        yield return null;
        LooseParent.SetActive(true);
        view.DOFade(1, 0.2f);

        LooseImage.gameObject.SetActive(true);
        RectTransform looseImageTransform = LooseImage.transform as RectTransform;
        Vector2 looseImagePos = looseImageTransform.anchoredPosition;
        looseImageTransform.anchoredPosition = new Vector2(looseImagePos.x, looseImagePos.y - 200f);
        LooseImage.DOColor(Color.white, 0.4f);
        looseImageTransform.DOAnchorPosY(looseImagePos.y, 0.4f);

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < playerData.Count(); i++)
        {
            players[i].gameObject.SetActive(true);
            players[i].GetComponent<CanvasGroup>().DOFade(1, 0.4f);
            yield return new WaitForSeconds(0.6f);
        }

        yield return new WaitForSeconds(0.5f);
        backButton.DOFade(1, 0.4f);

    }

    public void OnBackButtonPresses()
    {
        SceneManager.LoadScene((int)Scenes.MainMenu);
    }
    private void HideSpinner()
    {
        isLoaded = false;
        spinner.gameObject.SetActive(false);
    }
    void Update()
    {
        if (!isLoaded)
            spinner.Rotate(Vector3.back * Time.deltaTime * 100f);

    }
}