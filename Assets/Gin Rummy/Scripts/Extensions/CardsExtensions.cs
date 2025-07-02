using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CardsExtensions {

    private static Transform cardHolder;


    public static void DealCard(this Card card, CardDestination cardDestination)
    {
        AssignCardHolder();
        card.DealCard(cardDestination,cardHolder);
    }

    public static void DealCard(this Card card, CardDestination cardDestination, Transform parent)
    {
        SoundManager.instance.PlayPickCardSound();
        card.SetReturnPoint(cardDestination);
        card.SetCardParent(parent);
        card.transform.localScale = parent.localScale;
        card.MoveCard(Constants.DEAL_ANIM_TIME);
    }

    public static void SetCardHolderAsParent(this Card card)
    {
        AssignCardHolder();
        card.SetCardParent(cardHolder);
    }

    private static void AssignCardHolder()
    {
        if(cardHolder == null)
            cardHolder = GameObject.FindGameObjectWithTag("CardHolder").transform;
    }

    public static void StopCardsDragging()
    {
        AssignCardHolder();
        cardHolder.BroadcastMessage("ForceStopDrag", SendMessageOptions.DontRequireReceiver);
    }

    public static Card TakeCard(this Transform transform, Hand hand)
    {
        Card card = transform.GetFirstAvailableCard();
        if (card != null)
        {
            float cardRotationAngle = 0;
            Vector3 moveDestination = hand.GetNextAvailablePosition(out cardRotationAngle);
            CardDestination cardDestination = new CardDestination(hand.transform, moveDestination, cardRotationAngle);
            card.DealCard(cardDestination);
        }
        else
        {
            Debug.LogError("There is no Card to take");
        }
        return card;
    }

    public static Card GetFirstAvailableCard(this Transform transform)
    {
        for (int i = transform.childCount-1; i >= 0 ; i--)
        {
            Card card = transform.GetChild(i).GetComponent<Card>();
            if (card != null)
                return card;
        }
        return null;
    }

    public static List<Card> SortCardsByValue(this List<Card> cards)
    {
        return   cards.OrderBy(x => x.cardValue).ToList();
    }

    public static List<Card> SortCardsByColor(this List<Card> cards)
    {
        return cards.OrderBy(x => x.cardColor).ThenBy(x => x.cardValue).ToList();
    }
    // ✅ Define GetSequencesScore() as an extension method for List<CardsSequence>
 
    public static void AddUniqueValue<T>(this List<T> list, T value)
    {
        if (list.Contains(value) == false)
            list.Add(value);
    }

    public static void AddUniqueValues<T>(this List<T> cards, List<T> cardsToAdd)
    {
        for (int i = 0; i < cardsToAdd.Count; i++)
        {
            cards.AddUniqueValue(cardsToAdd[i]);
        }
    }

    public static void InsertUniqueValues<T>(this List<T> cards, List<T> cardsToAdd)
    {
        for (int i = cardsToAdd.Count-1; i >= 0; i--)
        {
            cards.InsertUniqueValue(cardsToAdd[i]);
        }
    }

    public static void InsertUniqueValue<T>(this List<T> list, T value)
    {
        if (list.Contains(value) == false)
              list.Insert(0,value);
    }



    public static void RemoveCardsFromList(this List<Card> cards, List<Card> cardsToRemove)
    {
        for (int i = 0; i < cardsToRemove.Count; i++)
        {
            cards.Remove(cardsToRemove[i]);
        }
    }

    public static void KillSequences(this List<CardsSequence> sequences)
    {
        for (int i = 0; i < sequences.Count; i++)
        {
            sequences[i].DestroySequence();
        }

        sequences.Clear();
    }

    public static int GetSequencesScore(this List<CardsSequence> sequences)
    {
        int score = 0;
        for (int i = 0; i < sequences.Count; i++)
        {
            score += sequences[i].GetSequenceDeadwoodScore();
        }
        return score;
    }

    public static T GetRandomElementFromList<T>(this List<T> list)
    {
        if (list.Count == 0)
            return default(T);
        return list[Randomizer.GetRandomNumber(0, list.Count)];
    }

    public static T GetRandomElementFromArray<T>(this T[] array)
    {
        if (array.Length == 0)
            return default(T);
        return array[Randomizer.GetRandomNumber(0, array.Length)];
    }
}
