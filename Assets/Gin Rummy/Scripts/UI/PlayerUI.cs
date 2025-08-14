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
    
    // 🔹 NEW UI ELEMENTS FOR ENHANCED FUNCTIONALITY
    [Header("Enhanced UI Elements")]
    public Text cumulativeScoreText; // For Pool/Deals Rummy cumulative scoring
    public Image droppedIndicator; // Visual indicator when player has dropped
    public Image eliminatedIndicator; // Visual indicator when player is eliminated
    public Text gameStateText; // Text to show player status (Active/Dropped/Eliminated)

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
    
    // 🔹 NEW METHODS FOR ENHANCED FUNCTIONALITY
    public void UpdateCumulativeScore(int cumulativeScore)
    {
        if (cumulativeScoreText != null)
        {
            cumulativeScoreText.text = "Total: " + cumulativeScore;
        }
    }
    
    public void SetDroppedState(bool isDropped)
    {
        if (droppedIndicator != null)
        {
            droppedIndicator.gameObject.SetActive(isDropped);
        }
        
        if (gameStateText != null)
        {
            gameStateText.text = isDropped ? "DROPPED" : "ACTIVE";
            gameStateText.color = isDropped ? Color.red : Color.green;
        }
        
        // Dim the UI panel if player has dropped
        if (UIPanel != null)
        {
            Color panelColor = UIPanel.color;
            panelColor.a = isDropped ? 0.5f : 1.0f;
            UIPanel.color = panelColor;
        }
    }
    
    public void SetEliminatedState(bool isEliminated)
    {
        if (eliminatedIndicator != null)
        {
            eliminatedIndicator.gameObject.SetActive(isEliminated);
        }
        
        if (gameStateText != null)
        {
            gameStateText.text = isEliminated ? "ELIMINATED" : "ACTIVE";
            gameStateText.color = isEliminated ? Color.red : Color.green;
        }
        
        // Dim the UI panel if player is eliminated
        if (UIPanel != null)
        {
            Color panelColor = UIPanel.color;
            panelColor.a = isEliminated ? 0.3f : 1.0f;
            UIPanel.color = panelColor;
        }
    }
    
    public void UpdateGameModeSpecificUI(GameMode gameMode)
    {
        // Show/hide cumulative score based on game mode
        if (cumulativeScoreText != null)
        {
            cumulativeScoreText.gameObject.SetActive(gameMode == GameMode.Pool || gameMode == GameMode.Deals);
        }
        
        // Update score display based on game mode
        RefreshScoreText();
    }
    
    public void ResetEnhancedUI()
    {
        SetDroppedState(false);
        SetEliminatedState(false);
        UpdateCumulativeScore(0);
        
        if (gameStateText != null)
        {
            gameStateText.text = "ACTIVE";
            gameStateText.color = Color.green;
        }
    }
}
