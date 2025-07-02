using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
public class CardsArea : MonoBehaviour {

    public Vector3 CalculateCardPos(int childNo, float cardSize, float totalOffset = 0)
    {
        Vector3 pos = new Vector3(childNo * (cardSize - Constants.CARD_SPACE_GAP) + cardSize / 2 + totalOffset, 0, 0) * transform.localScale.x;
        return transform.TransformPoint(pos);
    }

    List<Transform> properChildrenOrder = new List<Transform>();

    public virtual IEnumerator AnimateCardToArea(List<Card> cardsToAnimate, Transform startAnimPoint)
    {
        int cardIndex = 0;
        float totalOffset = 0;
        properChildrenOrder.Clear();
        for (int i = 0; i < cardsToAnimate.Count; i++)
        {
            Card card = cardsToAnimate[i];
            card.NormalColorCard();
            if (cardsToAnimate.Count > 1 && i > 0)
            {
                if (card.inSequence != cardsToAnimate[i - 1].inSequence)
                {
                    properChildrenOrder.Add(SpawnGap(cardIndex, ref totalOffset));
                    cardIndex++;
                }
            }
            RectTransform cardTransform = card.transform as RectTransform;
            CardDestination cardDestination = new CardDestination(transform, CalculateCardPos(cardIndex, cardTransform.sizeDelta.x, totalOffset), 0);
            card.isReversed = false;
            card.DealCard(cardDestination,startAnimPoint);
            card.transform.localScale = transform.localScale;
            card.transform.localPosition = Constants.vectorZero;
            properChildrenOrder.Add(card.transform);
            cardIndex++;
            if (i == cardsToAnimate.Count - 1)
                card.RegisterAction(SetChildrenCorrectOrder);
            yield return Constants.delayBetweenRoundEndCardAnims;
        }
    }

    private void SetChildrenCorrectOrder()
    {
        for (int i = 0; i < properChildrenOrder.Count; i++)
        {
            properChildrenOrder[i].SetAsLastSibling();
        }
    }

    private Transform SpawnGap(int childID, ref float totalOffset)
    {
        GameObject gap = Instantiate(Resources.Load("Gap"), transform.position, Quaternion.identity, transform) as GameObject;
        RectTransform gapRectTransform = gap.transform as RectTransform;
        gap.transform.position = CalculateCardPos(childID, gapRectTransform.sizeDelta.x, totalOffset);
        totalOffset += (gapRectTransform.sizeDelta.x - Constants.cardSize.x);
        return gapRectTransform;
    }


    public void ReorderChildren()
    {
        float totalOffset = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform cardChild = transform.GetChild(i) as RectTransform;
            Vector3 newPos = CalculateCardPos(i, cardChild.sizeDelta.x, totalOffset);
            totalOffset += (cardChild.sizeDelta.x - Constants.cardSize.x);
            cardChild.AnimatePosition(newPos);
        }
    }

    protected virtual void CardAnimationFinished(Card card)
    {
    }

    public List<Card> GetMyCards()
    {
        return GetComponentsInChildren<Card>().ToList();
    }
}
