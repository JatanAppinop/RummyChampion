using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BotHard : BotMedium {

    public BotHard(Deck deck) : base(deck)
    {
        SayBotDecision("Play with Bot - Hard");
    }

    protected override ITakeCard GetPile()
    {
        Card discardPileCard = discardPile.transform.GetFirstAvailableCard();
        if (discardPileCard == null)
        {
            Debug.LogError("Discard is empty!");
            return deck;
        }

        Card deckCard = deck.transform.GetFirstAvailableCard();
        if (deckCard == null)
        {
            Debug.LogError("Deck is empty!");
            return discardPile;
        }

        int discardPileCardScore;
        int deckPileCardScore;
        bool isDiscardWorthForMe = base.IsItWorthCardForMe(discardPileCard, out discardPileCardScore);
        bool isDeckWorthForMe = base.IsItWorthCardForMe(deckCard, out deckPileCardScore);

        SayBotDecision("Discard score: " + discardPileCardScore + " deck score: " + deckPileCardScore);
        if (discardPileCardScore > deckPileCardScore)
        {
            SayBotDecision("Biere z discard " + discardPile.name);
            return discardPile;
        }

        if (deckPileCardScore > discardPileCardScore)
        {
            SayBotDecision("Biere z decka " + deckCard.name);
            return deck;
        }

        SayBotDecision("Są równe sprawdzmy więc czy możemy zaszkodzić ziomkowi");
        if (IsItWorthCardForOpponent(deckCard))
        {
            SayBotDecision("Pasuje więc mu ją zabieram " + deckCard.name);
            return deck;
        }

        if (isDiscardWorthForMe)
        {
            SayBotDecision("Biere z discard może się przydać " + discardPile.name);
            return discardPile;
        }

        if (isDeckWorthForMe)
        {
            SayBotDecision("Biere z decka może się przydać " + deckCard.name);
            return deck;
        }

        SayBotDecision("zadna nie jest fajna to sobie wezmę z niższą wartością");
        if (discardPileCard.GetCardPointsValue() < deckCard.GetCardPointsValue())
        {
            SayBotDecision("Biere z discard mniejszą " + discardPile.name);
            return discardPile;
        }
        SayBotDecision("Biere z deck mniejszą " + deckCard.name);
        return deck;
    }

    public override void Discard()
    {
        if (HasLowerDeadwoodThanOpponent())
        {
            if (myHand.CanWinThisGame() == false)
            {
                GetAndDiscardCard();
            }
        }
        else
        {
            GetAndDiscardCard();
        }
    }

    
    private bool HasLowerDeadwoodThanOpponent()
    {
        Hand opponentHand = gameManager.thisPlayerHand;
        bool decision = myHand.GetDeadwoodScore() < opponentHand.GetDeadwoodScore() || myHand.GetDeadwoodScore() == 0;
        return decision;
    }

    private void GetAndDiscardCard()
    {
        cardOnAction = GetWasteCardToDiscard();
        ZoomOrDiscardCard();
    }

    protected override Card GetWasteCardToDiscard()
    {
        List<Card> cards = myHand.GetNotSequencedCards();
        if (cards.Count > 0 && CheckIfItIsNotLastNotSequencedButDrawFromDiscardPileCard(cards))
        {
            RemovePossibleWorthCards(cards);
            if (cards.Count == 0 || CheckIfItIsNotLastNotSequencedButDrawFromDiscardPileCard(cards) == false)
            {
                SayBotDecision("Wszystkie są przydatne więc odsortuj tylko karty wygodne dla przeciwnika");
                cards = myHand.GetNotSequencedCards();
                RemoveWorthOpponentCards(cards);
                if (cards.Count == 0 || CheckIfItIsNotLastNotSequencedButDrawFromDiscardPileCard(cards) == false)
                {
                    SayBotDecision("Wszystkie są przydatne dla przeciwnika, odrzuć losową");
                    cards = myHand.GetNotSequencedCards();
                }
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


    protected override void RemovePossibleWorthCards(List<Card> cards)
    {
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            if (IsItPossibleWorthCard(cards[i]) || IsItWorthCardForOpponent(cards[i]))
            {
                cards.Remove(cards[i]);
                continue;
            }
        }
    }

    private void RemoveWorthOpponentCards(List<Card> cards)
    {
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            if (IsItWorthCardForOpponent(cards[i]))
            {
                cards.Remove(cards[i]);
                continue;
            }
        }
    }

    protected override bool IsItWorthCardForMe(Card card, out int possibleScore)
    {
        return base.IsItWorthCardForMe(card,out possibleScore) || IsItWorthCardForOpponent(card);
    }

    private bool IsItWorthCardForOpponent(Card card)
    {
        Hand opponentHand = gameManager.thisPlayerHand;
        int currentScore = opponentHand.GetSequencesScore();
        List<Card> handCards = opponentHand.GetCardsFromZone();
        handCards.Add(card);
        int firstColorSeqScore = opponentHand.FindSequencesByColorFirst(handCards);
        int firstValueSeqScore = opponentHand.FindSequencesByValueFirst(handCards);
        opponentHand.AutomaticFindSequences();

        bool decision = firstColorSeqScore > currentScore || firstValueSeqScore > currentScore;
        string dec = decision ? "biore bo jestem psem ogrodnika " : " nie biore";
        SayBotDecision("Decyzja dla " + card.name + " Obecnie przeciwnik ma: " + currentScore + " Jak mu zostawie to w kolorach bedzie miec: " + firstColorSeqScore + " a w wartosciach " + firstValueSeqScore
            + " wiec moja decyzja biore " + dec);
        
        return decision;
    }
}
