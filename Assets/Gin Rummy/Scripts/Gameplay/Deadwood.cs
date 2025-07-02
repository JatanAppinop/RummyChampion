using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Deadwood : MonoBehaviour, IResettable
{
    public Text deadwoodStatusTxt;
    public Action OnDeadwoodAnimationFinishedCB;
    private int deadwoodPoints;
    private Hand hand;
    private GameManager gameManager;
    private WinGameButton winGameBtn;
    private GameObject scorePrefab;
    private bool canWin;
    private List<ScoreAnimation> scoreAnimations = new List<ScoreAnimation>();

    private void Awake()
    {
        winGameBtn = GameObject.Find("KnockBtn").GetComponent<WinGameButton>();
        scorePrefab = Resources.Load("Score") as GameObject;
        hand = GetComponent<Hand>();
        hand.OnCardsPositionRefreshCB += RefreshDeadwoodPoints;
        gameManager = FindObjectOfType<GameManager>();
        RegisterResetable();
    }

    private void Start()
    {
        winGameBtn.ShowKnockBtn(false);
    }

    private void RefreshDeadwoodText()
    {
        if (deadwoodStatusTxt != null)
        {
            deadwoodStatusTxt.text = "SCORE: " + deadwoodPoints;
        }
    }

    private void RefreshDeadwoodPoints()
    {
        List<Card> cardsInHand = hand.GetCardsFromZone();
        deadwoodPoints = 0;
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            Card card = cardsInHand[i];
            if (card.inSequence == null)
            {
                deadwoodPoints += card.GetCardPointsValue();
            }
        }

        RefreshDeadwoodText();
        bool knockState = CheckIfKnockIsAvailable();
        if (knockState)
            Debug.LogError($"Ready to Knock");
        winGameBtn.ShowKnockBtn(knockState);
    }

    IEnumerator pointsAnimation;

    public void RunDeadwoodPointsAnimation()
    {
        pointsAnimation = ShowDeadwoodPointsAnimation();
        StartCoroutine(pointsAnimation);
    }

    private IEnumerator ShowDeadwoodPointsAnimation()
    {
        scoreAnimations.Clear();
        List<Card> cardsInHand = hand.GetCardsFromZone();
        deadwoodPoints = 0;
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            Card card = cardsInHand[i];
            if (card.inSequence == null)
            {
                int score = card.GetCardPointsValue();
                deadwoodPoints += score;
                GameObject scoreGO = Instantiate(scorePrefab, card.transform.position, card.transform.rotation, card.transform);
                ScoreAnimation scoreAnimation = scoreGO.GetComponent<ScoreAnimation>();
                scoreAnimation.SetScore(score);
                scoreAnimations.Add(scoreAnimation);
                RefreshDeadwoodText();
                yield return Constants.delayBetweenDeadwoodCardAnim;
            }
        }
        OnDeadwoodAnimationFinishedCB.RunAction();
    }

    public void StopDeadwoodScoreAnimation()
    {
        StopCoroutine(pointsAnimation);
        for (int i = 0; i < scoreAnimations.Count; i++)
        {
            if (scoreAnimations[i] != null)
                Destroy(scoreAnimations[i].gameObject);
        }
    }
    private bool CheckIfKnockIsAvailable()
    {
        // Check if the hand and player are valid
        if (hand == null || hand.playerOfThisHand == null)
            return false;

        // Get the highest card value in the hand
        Card card = hand.playerOfThisHand.GetBiggestCardFromHand();
        int highestCardValueInHand = card != null ? (int)card.GetCardPointsValue() : 0;

        // Calculate the knock score
        int knockScore = deadwoodPoints - highestCardValueInHand;

        // Check if the knock score is within the allowed limit
        bool hasMinPointsToKnock = knockScore <= Constants.POINTS_REQUIRED_TO_KNOCK;

        // Check if the player has the maximum number of cards
        bool hasAllCardsInHand = hand.GetCardsFromZone().Count == Constants.MAX_CARDS_NUMBER;

        // Determine the win type
        WinType winType = GetWinType();
        winGameBtn.SetWinType(winType, knockScore);

        // Check if the player can win now
        canWin = hasMinPointsToKnock && hasAllCardsInHand;

        // Return whether knocking is available
        return canWin && gameManager.IsThisGamePlayerAndTimeIsNotOver();
    }

    private WinType GetWinType()
    {
        List<Card> cards = hand.GetNotSequencedCards();
        if (cards.Count > 1)
            return WinType.Knock;
        if (cards.Count == 1)
            return WinType.Gin;
        else
            return WinType.BigGin;
    }

    public void ResetState()
    {
        deadwoodPoints = 0;
        RefreshDeadwoodPoints();
    }

    public void RegisterResetable()
    {
        gameManager.RegisterResettable(this);
    }

    public bool PlayerCanWinNow()
    {
        if (canWin)
        {
            winGameBtn.Click();
        }
        return canWin;
    }

    public int GetDeadwoodScore()
    {
        RefreshDeadwoodPoints();
        return deadwoodPoints;
    }


}
