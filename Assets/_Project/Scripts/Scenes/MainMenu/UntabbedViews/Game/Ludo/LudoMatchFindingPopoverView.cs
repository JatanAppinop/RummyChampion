using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Appinop;
using AssetKits.ParticleImage;
using DG.Tweening;
using SecPlayerPrefs;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LudoMatchFindingPopoverView : PopoverView
{
    [SerializeField] private bool isLoaded = false;
    private RectTransform rectTransform;

    [Header("references")]
    [SerializeField] List<CanvasGroup> photos;
    [SerializeField] RectTransform photo;
    [SerializeField] ParticleImage particleImage;
    [Header("Labels")]
    [SerializeField] TextMeshProUGUI timerLabel;
    [SerializeField] TextMeshProUGUI statusLabel;

    [Header("Profile Photo")]
    [SerializeField] ProfilePhoto userPhoto;
    [SerializeField] ProfilePhoto opponentPhoto;
    [SerializeField] List<ProfilePhoto> opponentPhotos;

    [Space]
    [SerializeField] Image vs;

    //Coroutines
    IEnumerator StartAnimationRoutine;
    IEnumerator AnimatePhotoRoutine;
    IEnumerator TimerRoutine;
    IEnumerator LabelRoutine;
    IEnumerator MatchFoundRoutine;
    IEnumerator MatchSearchRoutine;
    IEnumerator StartMatchSearchingRoutine;

    //Private References
    private float timeOut = 60;
    private MultiplayerManager mManager;
    private string contestID;

    [SerializeField] private bool matchFound = false;

    private void Awake()
    {
        this.rectTransform = this.transform as RectTransform;


    }
    public override void Hide()
    {
        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Bottom, Animate: true, onComplete: () =>
        {
            ResetView();
            this.gameObject.SetActive(false);
        });
    }

    public override void Show()
    {
        if (!isLoaded)
        {
            isLoaded = true;
        }

        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Bottom);
        this.gameObject.SetActive(true);
        rectTransform.MoveToPosition(Vector2.zero, duration: 0.2f);

        vs.gameObject.SetActive(false);
        opponentPhoto.gameObject.SetActive(false);
        opponentPhotos.ForEach(o => o.gameObject.SetActive(false));

    }

    public override void Show(params KeyValuePair<string, object>[] args)
    {
        if (!isLoaded)
        {
            isLoaded = true;
        }

        photos.ForEach(p => p.alpha = 0.0f);

        this.gameObject.SetActive(true);
        rectTransform.MoveOutOfScreen(direction: Appinop.RectTransformExtensions.Direction.Bottom);
        this.gameObject.SetActive(true);
        rectTransform.MoveToPosition(Vector2.zero, duration: 0.2f, onComplete: () => StartIntroAnimation());
        contestID = args.GetValueByKey<string>(Appinop.Constants.KContestId);
        Debug.Log("Contest ID : " + contestID);

        UpdateTimer(timeOut);

        userPhoto.UpdateData(UserDataContext.Instance.profileData);

        StartSearchingMatch(contestID);

    }

    private void ResetView()
    {
        ResetAnimations();
        mManager.Disconnect();
        Destroy(mManager, 0.1f);
    }

    public override void OnFocus(bool dataUpdated = false)
    {

    }

    private void StartIntroAnimation()
    {
        particleImage.Play();

        StartAnimationRoutine = StartAnimation();
        StartCoroutine(StartAnimationRoutine);

        AnimatePhotoRoutine = AnimatePhotos();
        StartCoroutine(AnimatePhotoRoutine);

        TimerRoutine = Timer();
        StartCoroutine(TimerRoutine);

        UpdateStatusLabel("Finding Opponents");

    }

    private void ResetAnimations()
    {
        if (StartAnimationRoutine != null)
        {
            StopCoroutine(StartAnimationRoutine);
            StartAnimationRoutine = null;
        }
        photo.localScale = Vector3.one;

        if (AnimatePhotoRoutine != null)
        {
            StopCoroutine(AnimatePhotoRoutine);
            AnimatePhotoRoutine = null;
        }

        photos.ForEach(p => p.alpha = 0.0f);

        if (TimerRoutine != null)
        {
            StopCoroutine(TimerRoutine);
            TimerRoutine = null;
        }
        UpdateTimer(timeOut);

        if (LabelRoutine != null)
        {
            StopCoroutine(LabelRoutine);
            LabelRoutine = null;
        }
        statusLabel.SetText("Loading");

        if (MatchFoundRoutine != null)
        {
            StopCoroutine(MatchFoundRoutine);
            MatchFoundRoutine = null;
        }

        if (MatchSearchRoutine != null)
        {
            StopCoroutine(MatchSearchRoutine);
            MatchSearchRoutine = null;
        }
        if (StartMatchSearchingRoutine != null)
        {
            StopCoroutine(StartMatchSearchingRoutine);
            StartMatchSearchingRoutine = null;
        }

        particleImage.Stop();

        (userPhoto.transform as RectTransform).anchoredPosition = Vector3.zero;
        userPhoto.ShowNameTag(false);
        (opponentPhoto.transform as RectTransform).anchoredPosition = Vector3.zero;
        userPhoto.ShowNameTag(false);
        opponentPhoto.gameObject.SetActive(false);
        opponentPhotos.ForEach(o => o.gameObject.SetActive(false));
        vs.gameObject.SetActive(false);

        matchFound = false;
        particleImage.loop = true;

        mManager.onMatchFound.RemoveListener(onMatchFound);

        photo.localScale = Vector3.zero;
        UpdateTimer(0f);
        statusLabel.SetText("Loading");
    }
    IEnumerator StartAnimation()
    {
        yield return new WaitForSeconds(0.1f);
        photo.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        yield return null;

    }

    private void StartSearchingMatch(string contestId)
    {
        StartMatchSearchingRoutine = FindMatch(contestId);
        StartCoroutine(StartMatchSearchingRoutine);
    }

    IEnumerator FindMatch(string contestId)
    {
        if (mManager == null)
        {
            mManager = this.gameObject.AddComponent<MultiplayerManager>();
        }
        yield return new WaitForSeconds(3);
        mManager.onMatchFound.AddListener(onMatchFound);
        mManager.ConnectServer(APIServices.Instance.GetSocketUrl, contestId);
    }
    IEnumerator AnimatePhotos()
    {

        yield return new WaitForSeconds(2f);
        while (true) // Infinite loop
        {
            foreach (CanvasGroup photo in photos)
            {
                photo.DOFade(1f, 2f);
                yield return new WaitForSeconds(2.5f);
                photo.DOFade(0f, 1.5f);
                yield return new WaitForSeconds(0.5f);
            }
        }

    }

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(1f);
        float timer = timeOut;

        while (timer > 0 && !matchFound)
        {
            UpdateTimer(timer);

            yield return new WaitForSeconds(1f);

            timer -= 1f;
        }

        if (!matchFound)
        {
            UpdateTimer(0f);
            onMatchNotFound();
        }

    }

    IEnumerator MatchFoundAnimation(int playercount)
    {
        particleImage.loop = false;

        if (AnimatePhotoRoutine != null)
        {
            StopCoroutine(AnimatePhotoRoutine);
            AnimatePhotoRoutine = null;
        }

        photos.ForEach(p => DOTween.Kill(p));
        photos.ForEach(p => p.DOFade(0f, 0.2f));
        yield return new WaitForSeconds(0.5f);

        if (playercount == 2)
        {

            opponentPhoto.gameObject.SetActive(true);
            UpdateStatusLabel("Match Found");
            yield return new WaitForSeconds(0.5f);
            (userPhoto.transform as RectTransform).DOAnchorPosX(-260, 0.2f).SetEase(Ease.OutQuad).OnComplete(() => userPhoto.ShowNameTag(true));
            (opponentPhoto.transform as RectTransform).DOAnchorPosX(260, 0.2f).SetEase(Ease.OutQuad).OnComplete(() => opponentPhoto.ShowNameTag(true));
            vs.color = Color.clear;
            (vs.transform as RectTransform).localScale = Vector3.one * 100;
            vs.gameObject.SetActive(true);
            (vs.transform as RectTransform).DOScale(Vector3.one, 0.2f).SetEase(Ease.InBack);
            vs.DOColor(Color.white, 0.2f);
        }
        else
        {
            UpdateStatusLabel("Match Found");
            yield return new WaitForSeconds(0.5f);
            (userPhoto.transform as RectTransform).DOAnchorPosY(300, 0.2f).SetEase(Ease.OutQuad).OnComplete(() => userPhoto.ShowNameTag(true));
            opponentPhotos.ForEach(o =>
            {
                o.GetComponent<CanvasGroup>().alpha = 0;
                o.gameObject.SetActive(true);
                o.GetComponent<CanvasGroup>().DOFade(1, 0.2f);
                (o.transform as RectTransform).DOAnchorPosY(-250, 0.2f).SetEase(Ease.OutQuad).OnComplete(() => o.ShowNameTag(true));

            });
            vs.color = Color.clear;
            (vs.transform as RectTransform).localScale = Vector3.one * 100;
            vs.gameObject.SetActive(true);
            (vs.transform as RectTransform).DOScale(Vector3.one, 0.2f).SetEase(Ease.InBack);
            vs.DOColor(Color.white, 0.1f);
        }

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(startGame());

    }

    private void ShowMatchFoundAnimation(int playercount)
    {
        if (MatchFoundRoutine != null)
        {
            StopCoroutine(MatchFoundRoutine);
        }
        MatchFoundRoutine = MatchFoundAnimation(playercount);
        StartCoroutine(MatchFoundRoutine);

    }

    private void onMatchFound(string matchId)
    {
        Debug.Log("Match Found : " + matchId);
        matchFound = true;
        //ShowMatchFoundAnimation();
        LoadMatchData(matchId);
    }
    private void onMatchNotFound()
    {
        Debug.LogError("Match Not Found : ");

        ResetView();

        AlertSlider.Instance.Show("<b>Match Not Found.</b>\nWe were not able to find match for you.", "Try Again", "Cancel")
        .OnPrimaryAction(() =>
        {
            StartIntroAnimation();
            StartSearchingMatch(contestID);
        })
        .OnSecondaryAction(() =>
        {
            PopoverViewController.Instance.GoBackTo(2);
        });
    }
    private async void LoadMatchData(string matchId)
    {

        var matchResponce = await APIServices.Instance.GetAsync<Match>(APIEndpoints.getMatch + matchId);
        if (matchResponce == null && !matchResponce.success)
        {
            return;
        }

        // Save Match ID
        SecurePlayerPrefs.SetString(Appinop.Constants.KMatchId, matchId);
        mManager.Disconnect();

        if (matchResponce.data.players.Count() == 2)
        {
            string opponentID = matchResponce.data.players.Find(p => p != UserDataContext.Instance.UserData._id);
            var oppUserResponce = await APIServices.Instance.GetAsync<User>((APIEndpoints.getUser + opponentID));
            if (oppUserResponce != null && oppUserResponce.success)
            {
                ProfileData oppProfileData = new ProfileData();
                oppProfileData.phNo = oppUserResponce.data.mobileNumber;
                oppProfileData.fullName = oppUserResponce.data.username;
                oppProfileData.photo = ProfilePhotoHelper.Instance.GetProfileSprite(oppUserResponce.data.profilePhotoIndex);
                oppProfileData.backgroundColor = ProfilePhotoHelper.Instance.GetBackdropColor(oppUserResponce.data.backdropIndex);
                opponentPhoto.UpdateData(oppProfileData);
            }
        }
        else
        {
            List<string> opponents = matchResponce.data.players.FindAll(p => p != UserDataContext.Instance.UserData._id);
            for (int i = 0; i < opponents.Count(); i++)
            {
                var oppUserResponce = await APIServices.Instance.GetAsync<User>((APIEndpoints.getUser + opponents[i]));
                if (oppUserResponce != null && oppUserResponce.success)
                {
                    ProfileData oppProfileData = new ProfileData();
                    oppProfileData.phNo = oppUserResponce.data.mobileNumber;
                    oppProfileData.fullName = oppUserResponce.data.username;
                    oppProfileData.photo = ProfilePhotoHelper.Instance.GetProfileSprite(oppUserResponce.data.profilePhotoIndex);
                    oppProfileData.backgroundColor = ProfilePhotoHelper.Instance.GetBackdropColor(oppUserResponce.data.backdropIndex);
                    opponentPhotos[i].UpdateData(oppProfileData);
                }
            }

        }

        ShowMatchFoundAnimation(matchResponce.data.players.Count());
    }

    IEnumerator startGame()
    {
        UpdateStatusLabel("Starting Game in");
        int i = 3;
        while (i > 0)
        {
            timerLabel.SetText(i.ToString());
            yield return new WaitForSeconds(1f);
            i--;
        }

        TableData tableData = DataContext.Instance.contestsData.FirstOrDefault(detail => detail._id == contestID);

        if (tableData.game == "Rummy")
            SceneManager.LoadScene("Game");
        else
            SceneManager.LoadScene("GameScene");

    }

    private void UpdateTimer(float time)
    {

        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        // Update timer label
        timerLabel.text = string.Format("{0:00}:{1:00}", minutes, seconds);

    }

    private void UpdateStatusLabel(string text)
    {
        if (LabelRoutine != null)
        {
            StopCoroutine(LabelRoutine);
            LabelRoutine = null;
        }
        LabelRoutine = UpdateLabelText(text);
        StartCoroutine(LabelRoutine);

    }
    IEnumerator UpdateLabelText(string newText)
    {
        statusLabel.text = "";
        foreach (char letter in newText)
        {
            statusLabel.text += letter;
            yield return new WaitForSeconds(0.005f);
        }
    }

    public void OnBackButtonPressed()
    {
        PopoverViewController.Instance?.GoBackTo(2);
    }
    private void OnDestroy()
    {
        mManager.onMatchFound.RemoveListener(onMatchFound);
    }
}
