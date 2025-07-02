using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasGroup))]
public class DropZone : MonoBehaviour, IDropHandler {

	private CanvasGroup canvasGroup;
	protected GameManager gameManager;
    protected Deck deck;

	public virtual void AssignNewChild(Transform child)
	{
		SoundManager.instance.PlayDropCardSound();
		child.SetParent(transform,true);
        child.AnimateParentScale();
    }

    public virtual void AssignNewChild(Transform child,  int cardOrder)
	{
		AssignNewChild(child);
	}

	public virtual void UnparentChild(Transform child, Transform newParent)
	{
		child.SetParent(newParent);
        child.AnimateParentScale();
	}

	protected virtual void Awake() {
		canvasGroup = GetComponent<CanvasGroup>();
		gameManager = FindObjectOfType<GameManager>();
        deck = FindObjectOfType<Deck>();
    }

    virtual public void OnDrop(PointerEventData eventData) {
		if (eventData != null)
		{
			if (gameManager.IsValidTimeToDiscard())
			{
				Card c = eventData.pointerDrag.GetComponent<Card>();
				if(c != null && gameManager.IsValidCardToDiscard(c) && c.previousParent != this.transform)
				{
					gameManager.currentPlayer.RegisterNewCardMove(c);
					OnDrop(c);
				}
			}
		}
	}

	virtual public void OnDrop(Card card) {
		if (card != null) {
			CardDestination cardDestination = new CardDestination(transform, transform.position, 0);
			card.SetReturnPoint(cardDestination);
		}
	}

	

	public void LockZone() {
        if(canvasGroup != null)
		    canvasGroup.blocksRaycasts = false;
	}

	public virtual void UnlockZone() {
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
	}

    public List<Card> GetCardsFromZone()
    {
        List<Card> cards = new List<Card>();

        for (int i = 0; i < transform.childCount; i++)
        {
            Card c = transform.GetChild(i).GetComponent<Card>();
            if (c != null)
            {
                cards.Add(c);
            }
        }
        return cards;
    }

    public void ReturnCardsToDeck()
    {
        List<Card> cards = GetCardsFromZone();
        CardDestination cardDestination = new CardDestination(deck.transform, deck.transform.position, 0);

        for (int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            new Timer(i * 0.2f, () => {
                card.DealCard(cardDestination);
            });
        }
    }
}
