using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{

    public Image playerAvatar;
    public Text scoreText;
    public TextMeshProUGUI playerName;
    private TurnTimer timer;
    private Slider pointsSlider;
    private Image UIPanel;
    private int score;

    void Awake()
    {
        UIPanel = GetComponent<Image>();

        pointsSlider = GetComponentInChildren<Slider>();
        timer = GetComponentInChildren<TurnTimer>();
        RefreshScoreText();
    }

    public void AddScore(int scoreToSet)
    {
        score += scoreToSet;
        pointsSlider.value = score;
        RefreshScoreText();
    }

    public int GetScore()
    {
        return score;
    }

    private void RefreshScoreText()
    {
        Bid bid = GameManager.instance.bid;
        if (bid != null)
        {
            int pointsToWin = bid.pointsToWin;
            pointsSlider.maxValue = pointsToWin;

            string scoreTxt = score + " / " + pointsToWin;
            scoreText.text = scoreTxt;
        }
    }

    public void RefreshPlayerName(string nickname)
    {
        if (playerName != null)
        {
            playerName.SetText(nickname);
        }
    }


    public void ResetUI(Action timerAction)
    {
        score = 0;
        pointsSlider.value = 0;
        if (timer == null)
            timer = GetComponentInChildren<TurnTimer>();
        pointsSlider.gameObject.SetActive(true);
        timer.OnTimerFinishedCB = timerAction;
        RefreshScoreText();
    }

    public void RegisterTimerIsEndingAction(Action timerAction)
    {
        timer.OnTimerIsEndingCB = timerAction;
    }

    public void RefreshAvatar(int avatarID)
    {
        playerAvatar.sprite = ProfilePhotoHelper.Instance.GetProfileSprite(avatarID);
    }

    public void StartTimer()
    {
        timer.StartTimer();
    }

    public void StopTimer()
    {
        timer.StopTimer();
    }

    public bool TimeIsOver()
    {
        return timer.TimeIsOver();
    }
}
