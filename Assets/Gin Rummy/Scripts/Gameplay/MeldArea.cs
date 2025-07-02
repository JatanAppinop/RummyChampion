using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using DG.Tweening;

public class MeldArea : CardsArea
{
	Layoff layoffArea;
	Action OnDeadwoodAnimationStarted;

	protected void Awake()
	{
		layoffArea = FindObjectOfType<Layoff>();
	}

	public override IEnumerator AnimateCardToArea(List<Card> cardsToAnimate, Transform startAnimPoint)
	{
		ClearOldGaps();
		yield return base.AnimateCardToArea(cardsToAnimate, startAnimPoint);        
	}

	private void ClearOldGaps()
	{
		for (int i = transform.childCount-1; i >= 0 ; i--)
		{
			Destroy(transform.GetChild(i).gameObject);
		}
	}

	public IEnumerator CalculateLayoff(DeadwoodArea deadwood)
	{
		int layoffBonus = 0;
		List<CardsSequence> sequences = GetSequences();
		List<Card> opponentCards = deadwood.GetMyCards();
		Dictionary<Card, Card> cardsToLayoff = new Dictionary<Card, Card>();

		for (int i = 0; i < sequences.Count; i++)
		{
			for (int j = opponentCards.Count - 1; j >= 0; j--)
			{
				Card sequenceFittedCard;
				if (sequences[i].TryFitThisCardToSequence(opponentCards[j], out sequenceFittedCard))
				{
					cardsToLayoff.Add(opponentCards[j], sequenceFittedCard);
					layoffBonus += opponentCards[j].GetCardPointsValue();
					opponentCards.RemoveAt(j);
				}
			}
		}
		OnDeadwoodAnimationStarted = deadwood.ReorderChildren;
        if(cardsToLayoff.Count > 0)
         yield return AnimateLayoffCards(cardsToLayoff);//StartCoroutine(AnimateLayoffCards(cardsToLayoff));
        //return layoffBonus;
    }

	IEnumerator AnimateLayoffCards(Dictionary<Card, Card> cardsToLayoff)
	{
		int cardIndex = 0;
		foreach (KeyValuePair<Card,Card> c in cardsToLayoff)
		{
			Card card = c.Key;
			Card followCard = c.Value;
			card.transform.SetParent(transform.root);
			int siblingsIndex = followCard.transform.GetSiblingIndex();
			siblingsIndex = card.cardValue <= followCard.cardValue ? siblingsIndex : siblingsIndex+1;

            GameObject go = Instantiate(Resources.Load("FakeCard"), transform.position, Quaternion.identity, transform) as GameObject;
				go.transform.SetSiblingIndex(siblingsIndex);
			RectTransform cardRectTransform = card.transform as RectTransform;
			card.transform.DOMove(layoffArea.CalculateCardPos(cardIndex, cardRectTransform.sizeDelta.x), 1f)
				.OnComplete(() =>
				{
					Sequence animSequence = DOTween.Sequence();
					animSequence.Append(
					card.transform.DOMove(go.transform.position, 1f))
					.Insert(0, card.transform.DOLocalMoveY(0,0.5f)
						.OnComplete(() =>
						{
							Destroy(go);
                            card.CopySequenceSchema(followCard.inSequence);
							card.transform.SetParent(transform);
							card.transform.SetSiblingIndex(siblingsIndex);
						}));
				});
			cardIndex++;
			yield return  Constants.delayBetweenRoundEndCardAnims;
		}
		ReorderChildren();
		OnDeadwoodAnimationStarted.RunAction();
		yield return Constants.delayAfterLayoutCardAnim;

    }

    private List<CardsSequence> GetSequences()
	{
		List<CardsSequence> sequences = new List<CardsSequence>();
		List<Card> cards = GetMyCards();
		for (int i = 0; i < cards.Count; i++)
		{
			if (cards[i].inSequence != null)
				sequences.AddUniqueValue(cards[i].inSequence);
		}
		return sequences;
	}

	
}


	
