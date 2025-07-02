using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotMedium : AIPlayer {

    public BotMedium(Deck deck) : base(deck)
    {
        SayBotDecision("Play with Bot - Medium");

    }

    private int score;

    public override void Discard()
    {
        if (myHand.CanWinThisGame() == false)
        {
            cardOnAction = GetWasteCardToDiscard();
            ZoomOrDiscardCard();
        }
    }

    protected virtual Card GetWasteCardToDiscard()
    {
        List<Card> cards = myHand.GetNotSequencedCards();
        if (cards.Count > 0 && CheckIfItIsNotLastNotSequencedButDrawFromDiscardPileCard(cards))
        {
            RemovePossibleWorthCards(cards);
            if(cards.Count == 0 || CheckIfItIsNotLastNotSequencedButDrawFromDiscardPileCard(cards) == false)
            {
                SayBotDecision("Wszystkie są przydatne więc wywal losową yolo");
                cards = myHand.GetNotSequencedCards();
            }
            Card card;
            do
            {
                card = cards.GetRandomElementFromList();
            }
            while (!IsValidCardToDiscard(card));
            return card;
        }
        return GetRandomCardFromHand();
    }

    protected virtual void RemovePossibleWorthCards(List<Card> cards)
    {
        for (int i = cards.Count -1; i >= 0; i--)
        {
            if(IsItPossibleWorthCard(cards[i]))
            {
                cards.Remove(cards[i]);
                continue;
            }
        }
    }

    public override void PassOrTake()
    {
        Card card = discardPile.transform.GetFirstAvailableCard();
        if(card == null)
        {
            Debug.LogError("Discard is empty!");
            return;
        }

        if (IsItWorthCardForMe(card, out score))
        {
            TakeCard(discardPile);
        }
        else
        {
            FullscreenTextMessage.instance.ShowText("Opponent passed");
            gameManager.OnPass();
        }
    }

    protected override ITakeCard GetPile()
    {
        Card card = discardPile.transform.GetFirstAvailableCard();
        if (card == null)
        {
            Debug.LogError("Discard is empty!");
            return deck;
        }


        if (IsItWorthCardForMe(card, out score))
        {
            return discardPile;
        }
        else
        {
            return deck;
        }
    }

    protected virtual bool IsItWorthCardForMe(Card card, out int possibleScore)
    {
        int currentScore = myHand.GetSequencesScore();
        List<Card> handCards = myHand.GetCardsFromZone();
        handCards.Add(card);
        int firstColorSeqScore = myHand.FindSequencesByColorFirst(handCards);
        int firstValueSeqScore = myHand.FindSequencesByValueFirst(handCards);
        myHand.AutomaticFindSequences();
        possibleScore = Math.Max(firstColorSeqScore, firstValueSeqScore);
        bool decision = firstColorSeqScore > currentScore || firstValueSeqScore > currentScore || IsItPossibleWorthCard(card);
        SayBotDecision("Decyzja dla karty: " +card.name +"Obecnie mam: " + currentScore + " Jak dobiore to w kolorach bede miec: " + firstColorSeqScore + " a w wartosciach " + firstValueSeqScore
            + " wiec moja decyzja biore " + decision);
        return decision;
    }

    protected bool IsItPossibleWorthCard(Card card)
    {
        List<Card> cards = myHand.GetCardsFromZone();
        for (int i = 0; i < cards.Count; i++)
        {
            Card cardFromHand = cards[i];
            if (card == cardFromHand || cardFromHand.inSequence != null)
            { 
                continue;
            }

            if (IsPossibleSetElement (card, cardFromHand))
            {
                return true;
            }

            if( IsPossibleRunElement(card, cardFromHand))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPossibleSetElement(Card c1, Card c2)
    {
        return myHand.IsValidElementOfSetSequence(c1, c2);
    }

    private bool IsPossibleRunElement(Card c1, Card c2)
    {
        return myHand.IsValidElementOfRunSequence(c1, c2) || myHand.IsValidElementOfRunSequence(c2, c1);    
    }
}
