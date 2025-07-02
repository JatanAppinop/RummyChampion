using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class CardDestination
{
    public Transform parentToReturnTo;
    public Vector3 position;
    public float cardRotationZAngle;

    public CardDestination()
    {

    }

    public CardDestination(Transform parent, Vector3 pos, float angle)
    {
        parentToReturnTo = parent;
        position = pos;
        cardRotationZAngle = angle;
    }
}

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public Transform previousParent { get; protected set; }

    protected CanvasGroup canvasGroup;
    protected bool _isReversed;
    public bool isCardZoomed { get; protected set; }

    private CardDestination cardDestination;
    private Transform originAnimationTransform;

    private Vector3 screenPoint;
    private Vector3 clickOffset;

    protected bool canDrag;
    private bool isOnDrag;

    private GameObject placeHolder;
    private Transform placeholderParent;
    private int placeHolderSiblingIndex;
    private CardShadow cardShadow;

    private Vector3 lastPosition;
    private Vector3 velocity;
    private Vector3 newRotation;

    protected Action OnAnimationFinishCB;
    public Action OnPlaceHolderChangePos;

    public void RegisterAction(Action cb)
    {
        OnAnimationFinishCB += cb;
    }

    public void UnregisterAction(Action cb)
    {
        OnAnimationFinishCB -= cb;
    }

    public virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        ApplicationPauseWatcher.RegisterOnAppPauseCB(ForceStopDrag);
        originAnimationTransform = GameObject.FindGameObjectWithTag("CardHolder").transform;
        cardDestination = new CardDestination();
        cardShadow = GetComponent<CardShadow>();
    }

    private void ForceStopDrag()
    {
        if (isOnDrag)
        {
            OnEndDrag(null);
        }
    }

    public void UnlockCard(bool state)
    {
        canvasGroup.blocksRaycasts = state;
    }

    protected Vector3 beforeZoomLocalPos;

    public void Unzoom(Draggable card)
    {
        if (card != this && isCardZoomed)
        {
            isCardZoomed = false;
            float scale = 1;
            if ((this as Card).inSequence != null)
                scale = Constants.SEQUENCED_CARDS_SCALE_FACTOR;
            transform.AnimateParentScale(scale);
            transform.AnimateLocalPosition(beforeZoomLocalPos, Constants.QUICK_ANIM_TIME);
        }
    }



    virtual public void OnBeginDrag(PointerEventData eventData)
    {
        if (CanDrag())
        {
            Unzoom(null);
            transform.parent.BroadcastMessage("Unzoom", this);
            transform.DOKill();
            SoundManager.instance.PlayPickCardSound();
            CreateNewPlaceholder();
            previousParent = transform.parent;
            CardDestination cardDestination = new CardDestination(previousParent, transform.position, transform.localRotation.eulerAngles.z);
            SetReturnPoint(cardDestination);
            ChangeCardParent(originAnimationTransform);
            transform.AnimateLocalRotation(Constants.vectorZero);
            placeholderParent = cardDestination.parentToReturnTo;
            cardShadow.StartShadowAnimation();
            transform.DOLocalMoveZ(Constants.DRAG_CARD_Z_DISTANCE, Constants.DRAG_CARD_Z_ANIM_TIME);

            if (eventData != null)
            {
                isOnDrag = true;
                GrowCard();
                screenPoint = transform.position;
                clickOffset = screenPoint - eventData.pressPosition.ConvertOverlayPositonToCameraSpace();
            }

            UnlockCard(false);
        }
    }

    private void CreateNewPlaceholder()
    {
        placeHolder = new GameObject();
        placeHolder.transform.SetParent(transform.parent);
        placeHolder.transform.localPosition = transform.localPosition;
    }

    private void GrowCard()
    {
        transform.DOScale(Constants.vectorZoom, Constants.CARD_GROW_ANIM_SPEED);
    }

    private void ShrinkCard()
    {
        transform.DOScale(Constants.vectorOne, Constants.CARD_GROW_ANIM_SPEED);
    }


    private bool CanDrag()
    {
        Hand hand = transform.parent.GetComponent<Hand>();
        if (hand != null)
        {
            canDrag = true;
            hand.LockZone();
        }
        else
        {
            canDrag = false;
        }
        return canDrag;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canDrag && isOnDrag)
        {
            Vector3 newPos = eventData.position.ConvertOverlayPositonToCameraSpace() + clickOffset;
            newPos.z = transform.position.z;
            transform.position = newPos;
            MovePlaceHolder();
        }
    }

    private void Update()
    {
        if (isOnDrag)
        {
            RotationDragEffect();
        }
    }

    private void RotationDragEffect()
    {
        velocity = (transform.position - lastPosition) / Time.deltaTime;
        CalculateNewCardRotation();
        transform.AnimateLocalRotation(newRotation);
        velocity -= velocity * Constants.DRAG_STRENGTH * Time.deltaTime;
        lastPosition = transform.position;
    }

    void CalculateNewCardRotation()
    {
        newRotation.x = Mathf.Clamp(Constants.DRAG_MAX_ANGLE * velocity.y, -Constants.DRAG_MAX_ANGLE, Constants.DRAG_MAX_ANGLE);
        newRotation.y = Mathf.Clamp(-Constants.DRAG_MAX_ANGLE * velocity.x, -Constants.DRAG_MAX_ANGLE, Constants.DRAG_MAX_ANGLE);
    }

    private void MovePlaceHolder()
    {
        if (placeHolder.transform.parent != placeholderParent)
            placeHolder.transform.SetParent(placeholderParent);

        int newSiblingIndex = placeholderParent.childCount;
        bool firstSequencedCardMarked = false;
        for (int i = 0; i < placeholderParent.childCount; i++)
        {
            if (CheckIfThisChildIsOnSequence(placeholderParent.GetChild(i)))
            {
                if (firstSequencedCardMarked)
                    continue;
                firstSequencedCardMarked = true;
            }
            if (transform.position.x < placeholderParent.GetChild(i).position.x)
            {
                newSiblingIndex = i;
                if (placeHolder.transform.GetSiblingIndex() < newSiblingIndex)
                    newSiblingIndex--;
                break;
            }
        }
        if (placeHolder.transform.GetSiblingIndex() != newSiblingIndex)
        {
            placeHolder.transform.SetSiblingIndex(newSiblingIndex);
            placeHolderSiblingIndex = newSiblingIndex;
            OnPlaceHolderChangePos.RunAction();
        }
    }

    private bool CheckIfThisChildIsOnSequence(Transform child)
    {
        Card card = child.GetComponent<Card>();
        if (card != null)
            return card.inSequence != null;
        return false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canDrag && isOnDrag)
        {
            isOnDrag = false;
            ShrinkCard();
            DestroyPlaceHolder();
            MoveCard(Constants.MOVE_ANIM_TIME);
        }
    }

    private void DestroyPlaceHolder()
    {
        placeHolder.transform.SetParent(transform.root);
        Destroy(placeHolder);
        if (cardDestination.parentToReturnTo == previousParent)
        {
            cardDestination.position = placeHolder.transform.position;
            cardDestination.cardRotationZAngle = placeHolder.transform.rotation.eulerAngles.z;
        }
        else
            OnPlaceHolderChangePos.RunAction();
        OnPlaceHolderChangePos = null;
    }

    public void MoveCard(float animTime)
    {
        UnlockCard(true);
        Unzoom(null);
        cardShadow.StartShadowAnimation();
        RotateCard(cardDestination.cardRotationZAngle, animTime);
        transform.DOMove(cardDestination.position, animTime).OnComplete(FinishCardAnim);
        Sequence animateZSequence = DOTween.Sequence();
        animateZSequence.Append(transform.DOLocalMoveZ(Constants.DRAG_CARD_Z_DISTANCE, animTime / 2f))
            .Append(transform.DOLocalMoveZ(0, animTime / 2f));
    }

    private void RotateCard(float angle, float animTime)
    {
        Vector3 rot = new Vector3(0, 0, cardDestination.cardRotationZAngle);

        bool shouldBeReversed = CheckIfCardShouldBeReversed();

        if ((shouldBeReversed && !_isReversed) || (!shouldBeReversed && _isReversed))
        {
            transform.DOLocalRotate(new Vector3(0, 90, 0) + rot, animTime / 3).OnComplete(() =>
            {
                (this as Card).FlipCard();
                transform.localRotation = Quaternion.Euler(new Vector3(0, -90, 0) + rot);
            });
            transform.DOLocalRotate(new Vector3(0, 0, 0) + rot, animTime / 3).SetDelay(animTime / 3 + Time.deltaTime).
                OnComplete(() => { transform.localRotation = Quaternion.Euler(rot); });
        }
        else
            transform.DOLocalRotate(rot, animTime);
    }

    private bool CheckIfCardShouldBeReversed()
    {
        return cardDestination.parentToReturnTo.GetComponent<BotHand>() == true || cardDestination.parentToReturnTo.GetComponent<Deck>() == true;
    }

    private void FinishCardAnim()
    {
        cardShadow.StopShadowAnimation();
        SetNewParent();
        UnlockCard(!_isReversed);
        Invoke("RunDelayedCalback", Time.deltaTime);
    }

    private void SetNewParent()
    {
        DropZone dropZone = cardDestination.parentToReturnTo.GetComponent<DropZone>();
        if (dropZone)
        {
            if (cardDestination.parentToReturnTo == previousParent)
            {
                dropZone.AssignNewChild(transform, placeHolderSiblingIndex);
                dropZone.UnlockZone();
            }
            else
                dropZone.AssignNewChild(transform);
        }
        else
        {
            SetCardParent(cardDestination.parentToReturnTo);
            CheckIfNewParentIsDeck(cardDestination.parentToReturnTo);
        }

        previousParent = cardDestination.parentToReturnTo;
    }

    private void ChangeCardParent(Transform newParent)
    {
        DropZone dropZone = previousParent.GetComponent<DropZone>();
        if (dropZone)
        {
            dropZone.UnparentChild(transform, newParent);
        }
        else
        {
             transform.SetParent(newParent, true);
            SetCardParent(newParent);
        }
    }

    private void CheckIfNewParentIsDeck(Transform newParent)
    {
        Deck deck = newParent.GetComponent<Deck>();
        if (deck != null)
        {
            deck.RefreshCardsCountText();
        }
    }

    void RunDelayedCalback()
    {
        OnAnimationFinishCB.RunAction();
    }

    public void SetReturnPoint(CardDestination newCardDestination)
    {
        cardDestination = newCardDestination;
    }

    public void SetCardParent(Transform parent)
    {
        transform.SetParent(parent, true);
        transform.AnimateParentScale();
    }

}
