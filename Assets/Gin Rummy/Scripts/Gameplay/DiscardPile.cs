using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

public class DiscardPile : DropZone, ITakeCard, IPointerDownHandler
{

    bool lockTakeCard;
    private void OnEnable()
    {
        RummySocketServer.Instance.OnReShuffle.AddListener(OnReshuffleCard);

    }
    private void OnDisable()
    {
        RummySocketServer.Instance.OnReShuffle.RemoveListener(OnReshuffleCard);

    }
    public void OnReshuffleCard(PlayerDeck cardData)
    {


        for (int j = this.transform.childCount - 1; j >= 0; j--)
        {
            Card card = transform.GetChild(j).GetComponent<Card>();
            if (card != null && cardData.topCard.code == card.cardCode)
                Debug.Log($"It's top card Moving Card: {card.cardCode} ");
            else
            {
                card.GetComponent<Image>().color = Constants.invisibleColor;
                card.isReversed = true;
                Debug.Log($"Moving Card: {card.cardCode} to playerDeck( Close Deck )");
                card.transform.SetParent(deck.transform, false);
                card.transform.localPosition = Vector3.zero;
                card.transform.localRotation = Quaternion.identity;
            }
        }

    }
    public override async void OnDrop(Card card)
    {

        if (card != null && card.previousParent != transform)
        {
            // float cardRotation = Randomizer.GetRandomNumber(-Constants.DISCARDPILE_CARD_MAX_ANGLE, Constants.DISCARDPILE_CARD_MAX_ANGLE);
            float cardRotation = 0;
            CardDestination cardDestination = new CardDestination(transform, transform.position, cardRotation);
            card.SetReturnPoint(cardDestination);
            card.SetCardHolderAsParent();
            if (gameManager.currentPlayer.playerId == UserDataContext.Instance.UserData._id)
            {

                await RummySocketServer.Instance.SendEvent(RummySocketEvents.drop_card, new Dictionary<string, string>() { { "cardCode", card.cardCode } });

                Debug.LogWarning("Card Drop Event Sent");
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (lockTakeCard)
            return;
        if (gameManager.IsValidTimeToTakeCard() || gameManager.IsPassOrTakePhase())
        {
            gameManager.CardTakenFromDiscardPile(transform.GetFirstAvailableCard().cardCode);
            gameManager.currentPlayer.TakeCard(this);
            RunLockCooldown();
            deck.RunLockCooldown();
        }
        else if (gameManager.IsValidTimeToDiscard())
        {
            gameManager.currentPlayer.DiscardZoomedCard();
        }
    }

    public void RunLockCooldown()
    {
        lockTakeCard = true;
        Invoke("UnlockHand", Constants.LOCK_HAND_TIME);
    }

    private void UnlockHand()
    {
        lockTakeCard = false;
    }

    public Card TakeCard(Hand hand)
    {
        Card card = transform.TakeCard(hand);
        if (card != null)
            card.NormalColorCard();
        return card;
    }

    public void FadeDiscardCards(bool fade)
    {
        Card[] cards = GetComponentsInChildren<Card>();
        for (int i = 0; i < cards.Length; i++)
        {
            if (fade)
                cards[i].DisableColorCard();
            else
                cards[i].NormalColorCard();
        }
    }

    public void Shake()
    {
        transform.DOShakeScale(1, 0.12f, 3, 45);
    }


}
