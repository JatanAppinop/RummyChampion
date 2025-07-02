using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using System;

public class Card : Draggable, IResettable, IPointerClickHandler
{
    public CardColor cardColor;
    public CardValue cardValue;
    public string cardCode;
    public int cardID { get; private set; }
    public CardsSequence inSequence { get; set; }
    public Action OnReverseAnimationFinishCB;

    private Image cardImage;
    private Image sequenceIndicator;

    private Sprite cardBackSprite;
    private List<Sprite> cardFrontSprite = new List<Sprite>();
    [SerializeField] private int currentCardSet;
    private RectTransform rt;
    public bool isJoker { get; private set; }  // NEW: Identify if this card is a Joker

    public bool isReversed
    {
        get
        {
            return _isReversed;
        }
        set
        {
            _isReversed = value;
            if (_isReversed)
            {
                sequenceIndicator.gameObject.SetActive(false);
                cardImage.sprite = cardBackSprite;
            }
            else
            {
                sequenceIndicator.gameObject.SetActive(true);
                cardImage.sprite = cardFrontSprite[currentCardSet];
            }
        }
    }

    public void RegisterResetable()
    {
        FindObjectOfType<GameManager>().RegisterResettable(this);
    }

    public void ResetState()
    {
        if (this == null)
        {
            Debug.LogWarning("Card object itself is null. Skipping ResetState.");
            return;
        }
        if (inSequence != null)
        {
            inSequence.DestroySequence();
            inSequence = null;
        }

        sequenceIndicator.color = Constants.invisibleColor;
        if (rt == null)
            rt = transform as RectTransform;

        rt.pivot = Constants.vectorHalf;
        rt.anchorMax = Constants.vectorHalf;
        rt.anchorMin = Constants.vectorHalf;

        isReversed = true;
        transform.DOKill();
        gameObject.SetActive(false);
        gameObject.SetActive(true);

        canvasGroup.blocksRaycasts = false;
        previousParent = transform.parent;

        NormalColorCard();
        ResetActions();
    }

    public int GetCardPointsValue()
    {
        return Mathf.Clamp((int)cardValue, Constants.MIN_POINTS_PER_CARD, Constants.MAX_POINTS_PER_CARD);
    }

    public void ResetActions()
    {
        OnAnimationFinishCB = null;
        OnReverseAnimationFinishCB = null;
    }


    public override void Awake()
    {
        base.Awake();
        cardID = int.Parse(gameObject.name.Substring(0, 2));
        //cardID = int.Parse(gameObject.name.Substring(0, 2));
        cardImage = GetComponent<Image>();
        rt = (transform as RectTransform);
        sequenceIndicator = transform.Find("ColorBar").GetComponent<Image>();
        LoadGraphics();
        RegisterResetable();
        ResetState();
        // NEW: Check if the card is a Joker
        isJoker = cardValue == CardValue.JOKER || cardValue == CardValue.JOKER;

    }

    private void LoadGraphics()
    {
        string path = "";
        for (int i = 0; i < Constants.CARD_SETS; i++)
        {
            path = GetCardPath(i);
            cardFrontSprite.Add(Resources.Load<Sprite>(path));
        }
        cardImage.sprite = cardFrontSprite[currentCardSet];
        path = "Cards/" + currentCardSet + "/cover/cover";
        cardBackSprite = Resources.Load<Sprite>(path);
    }
    //"Cards/0/RED/RED_JOKER"
    //"Cards/0/BLACK/BLACK_JOKER"
    private string GetCardPath(int setID)
    {
        return "Cards/" + setID + "/" + cardColor.ToString() + "/" + cardColor.ToString() + "_" + cardValue.ToString();
    }

    private void SwitchCardSet(int i)
    {
        currentCardSet = i;
        if (!_isReversed)
            cardImage.sprite = cardFrontSprite[currentCardSet];
    }



    public void RotateCard(float time)
    {
        ResetCardPositionAndRotation();
        transform.DOLocalRotate(new Vector3(0, 90, 0), time);
        transform.DOLocalRotate(new Vector3(0, 180, 0), time).SetDelay(time).OnComplete(FinishFlipCardAnim);
    }

    public void FlipCard(bool changeScale = false)
    {
        if (changeScale)
            transform.localScale = new Vector3(-1, 1, 1);
        isReversed = !isReversed;
    }

    private void FinishFlipCardAnim()
    {
        ResetCardPositionAndRotation();
        Invoke("RunOnReverseCallback", Time.deltaTime);
    }

    private void RunOnReverseCallback()
    {
        OnReverseAnimationFinishCB.RunAction();
    }

    private void ResetCardPositionAndRotation()
    {
        transform.localScale = Constants.vectorOne;
        transform.localRotation = Constants.quaternionIdentity;
        canvasGroup.blocksRaycasts = !_isReversed;
    }

    public void CopySequenceSchema(CardsSequence sequence)
    {
        if (sequence != null)
        {
            if (isJoker)
                SetSequenceIndicatorColor(Color.yellow); // NEW: Highlight Jokers
            else
                SetSequenceIndicatorColor(sequence.sequenceColor);
        }
    }

    public void SetSequenceIndicatorColor(Color c)
    {
        if (sequenceIndicator == null || c == null)
        {
            Debug.LogError("SetSequenceIndicatorColor -- null value!");
            return;
        }

        sequenceIndicator.color = c;
        if (cardIsDisabled)
            sequenceIndicator.color *= Constants.disabledCardColor;
    }

    private void SwitchSequenceIndicatorPosition(bool state)
    {
        float posY = 0;
        if (!state)
            posY = rt.sizeDelta.y - sequenceIndicator.rectTransform.sizeDelta.y;
        sequenceIndicator.rectTransform.anchoredPosition = new Vector2(0, posY);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ZoomCard();
    }

    public void ZoomCard(bool zoomIsForced = false)
    {
        if (transform.parent.GetComponent<Hand>() == null)
        {
            Debug.LogError("Cannot zoom card out of hand");
            return;
        }
        transform.parent.BroadcastMessage("Unzoom", this);
        if (isCardZoomed)
        {
            Unzoom(null);
            GameManager.instance.TryDiscardCard(this);
        }
        else if (zoomIsForced || (GameManager.instance.IsValidTimeToDiscard() && GameManager.instance.IsValidCardToDiscard(this)))
        {
            isCardZoomed = true;
            beforeZoomLocalPos = transform.localPosition;
            transform.AnimateParentScale(Constants.TAP_ZOOM_CARD_SIZE * transform.localScale.x);
            transform.AnimateLocalPosition(transform.localPosition + Vector3.up * Constants.SELECTED_MOVE_CARD_HEIGHT, Constants.QUICK_ANIM_TIME);
            GameManager.instance.TryAddThisCardAsThisPlayerZoomedCard(this);
        }
    }


    public void DisableColorCard()
    {
        cardIsDisabled = true;

        if (cardImage == null)
        {
            Debug.LogError("🚨 NullReferenceException: cardImage is null in DisableColorCard()");
            return; // Stop execution if cardImage is null
        }
        cardImage.color = Constants.disabledCardColor;

        if (sequenceIndicator == null)
        {
            Debug.LogError("🚨 NullReferenceException: sequenceIndicator is null in DisableColorCard()");
            return; // Stop execution if sequenceIndicator is null
        }
        SetSequenceIndicatorColor(sequenceIndicator.color);
    }

    bool cardIsDisabled;

    public void NormalColorCard()
    {
        cardIsDisabled = false;
        cardImage.color = Color.white;
        CopySequenceSchema(inSequence);
    }
}
