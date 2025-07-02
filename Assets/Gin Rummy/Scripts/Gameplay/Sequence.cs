using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

// Assuming you have an enum like:
// public enum TypeOfSequence { Run, Set, PureRun, ImpureRun, Trail, Pair }

[Serializable]
public class CardsSequence
{
    public TypeOfSequence sequenceType { get; private set; }
    private List<Card> cardsInSequence = new List<Card>();
    public Color sequenceColor { get; private set; }

    public CardsSequence(List<Card> cards, Color color, TypeOfSequence sequenceType)
    {
        this.sequenceType = sequenceType;
        cardsInSequence.AddRange(cards);
        sequenceColor = color;

        // Apply color / link sequence references:
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].SetSequenceIndicatorColor(sequenceColor);
            cards[i].inSequence = this;
        }
    }

    public bool ContainsJoker()
    {
        return cardsInSequence.Any(card => card.isJoker);
    }

    public int GetIndexOfCardInSeq(Card card)
    {
        return cardsInSequence.IndexOf(card);
    }

    public List<Card> GetCardsOfThisSequence()
    {
        return cardsInSequence;
    }

    public int GetSequenceDeadwoodScore()
    {
        int score = 0;
        foreach (var card in cardsInSequence)
        {
            // Jokers = 0 points
            if (!card.isJoker)
                score += card.GetCardPointsValue();
        }
        return score;
    }

    public void DestroySequence()
    {
        // Remove color and reference from each card in this sequence
        if (cardsInSequence != null)
        {
            foreach (var card in cardsInSequence)
            {
                card.SetSequenceIndicatorColor(Constants.invisibleColor);
                card.inSequence = null;
            }
        }
    }

    public bool HasThisCardInSequence(Card cardToCheck)
    {
        return cardsInSequence.Contains(cardToCheck);
    }

    /// <summary>
    /// Attempts to see if a new card can fit into this existing sequence 
    /// (based on sequenceType logic). If yes, sets 'sequenceFittedCard'
    /// to an anchor card already in the sequence.
    /// </summary>
    public bool TryFitThisCardToSequence(Card card, out Card sequenceFittedCard)
    {
        sequenceFittedCard = null;
        if (cardsInSequence.Count == 0) return false;

        switch (sequenceType)
        {
            case TypeOfSequence.Set:
                return CanFitSet(card, out sequenceFittedCard);

            case TypeOfSequence.Run:
            case TypeOfSequence.ImpureRun:
                return CanFitImpureRun(card, out sequenceFittedCard);

            case TypeOfSequence.PureRun:
                return CanFitPureRun(card, out sequenceFittedCard);

            case TypeOfSequence.Trail:
                return CanFitTrail(card, out sequenceFittedCard);

            case TypeOfSequence.Pair:
                return CanFitPair(card, out sequenceFittedCard);

            default:
                return false;
        }
    }

    /// <summary>
    /// Actually adds the card to this sequence if it fits. Then recolor & set references.
    /// </summary>
    public bool TryAddCardToSequence(Card card)
    {
        if (TryFitThisCardToSequence(card, out var fittedCard))
        {
            int index = cardsInSequence.IndexOf(fittedCard);

            // For runs: decide if card goes before or after
            if (NeedsToGoBefore(card, fittedCard))
                cardsInSequence.Insert(index, card);
            else
                cardsInSequence.Insert(index + 1, card);

            card.SetSequenceIndicatorColor(sequenceColor);
            card.inSequence = this;

            Debug.Log($"✅ Card {card.cardValue} added to the {sequenceType} sequence!");
            return true;
        }
        else
        {
            Debug.Log($"❌ Card {card.cardValue} cannot be added to this {sequenceType} sequence.");
            return false;
        }
    }

    /// <summary>
    /// Combine "TryAdd" and "Validate" in one go, if you want to 
    /// ensure the sequence is still valid after the addition.
    /// </summary>
    public bool TryAddAndValidate(Card card)
    {
        if (TryAddCardToSequence(card))
        {
            bool isValid = ValidateSequence();
            Debug.Log(isValid ? $"✅ The {sequenceType} sequence is valid!"
                              : $"❌ The {sequenceType} sequence is invalid.");
            return isValid;
        }
        return false;
    }

    /// <summary>
    /// Checks if the entire sequence is still valid according to 
    /// the rules for each TypeOfSequence.
    /// </summary>
    public bool ValidateSequence()
    {
        switch (sequenceType)
        {
            case TypeOfSequence.Set:
                return ValidateSet(3);

            case TypeOfSequence.Run:
                // treat "Run" the same as "ImpureRun" logic here
                return ValidateImpureRun();

            case TypeOfSequence.PureRun:
                return ValidatePureRun();

            case TypeOfSequence.ImpureRun:
                return ValidateImpureRun();

            case TypeOfSequence.Trail:
                // 3 or 4 cards of the same rank, jokers included
                return ValidateSet(3, maxCount: 4);

            case TypeOfSequence.Pair:
                // Exactly 2 of a kind
                return ValidateSet(2, maxCount: 2);

            default:
                return false;
        }
    }

    // -------------------------------------
    //      PRIVATE HELPER: FITTING LOGIC
    // -------------------------------------

    private bool CanFitSet(Card card, out Card anchor)
    {
        // "Set" means 3+ of the same rank (or jokers).
        // If we have at least 1 real card, the new card must share that rank or be a joker.
        anchor = cardsInSequence[0];
        var firstRealCard = cardsInSequence.FirstOrDefault(c => !c.isJoker);
        if (firstRealCard != null)
        {
            if (!card.isJoker && card.cardValue != firstRealCard.cardValue)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Allows 3 or 4 of the same rank (with jokers).
    /// If we already have 4 real cards, we can't add more real cards.
    /// </summary>
    private bool CanFitTrail(Card card, out Card anchor)
    {
        anchor = cardsInSequence.Last();
        var realCards = cardsInSequence.Where(c => !c.isJoker).ToList();

        // If we already have 4 real cards => can't fit more
        // (assuming "Trail" max is 4 total including jokers)
        if (realCards.Count >= 4) return false;

        // If there's at least one real card, all real cards must share the same rank
        if (realCards.Count > 0)
        {
            int rank = (int)realCards[0].cardValue;
            if (!card.isJoker && (int)card.cardValue != rank)
                return false;
        }
        return true;
    }

    private bool CanFitPair(Card card, out Card anchor)
    {
        anchor = null;

        // "Pair" means exactly 2 of a kind.
        // If we already have 2 real cards, we can't add more real ones.
        var realCards = cardsInSequence.Where(c => !c.isJoker).ToList();
        if (realCards.Count >= 2) return false;

        // If there's at least 1 real card, 
        // the new card must match that cardValue or be a joker.
        if (realCards.Count == 1)
        {
            if (!card.isJoker && card.cardValue != realCards[0].cardValue)
                return false;
        }

        // If adding the new card would exceed 2 total => fail
        if (cardsInSequence.Count >= 2) return false;

        // Otherwise, it can fit at the end
        anchor = cardsInSequence.Last();
        return true;
    }

    private bool CanFitImpureRun(Card card, out Card anchor)
    {
        anchor = null;
        // For an impure run, all real cards share the same suit/color.
        var realCards = cardsInSequence.Where(c => !c.isJoker).ToList();
        if (realCards.Count > 0 && !card.isJoker)
        {
            var suit = realCards[0].cardColor;
            if (card.cardColor != suit) return false;
        }

        // Allow Joker to fill gaps in the sequence
        if (card.isJoker)
        {
            anchor = cardsInSequence.Last();
            return true;
        }

        // Check if the card can fit into the sequence based on rank
        var sorted = cardsInSequence.OrderBy(c => c.cardValue).ToList();
        for (int i = 0; i < sorted.Count - 1; i++)
        {
            int gap = (int)sorted[i + 1].cardValue - (int)sorted[i].cardValue - 1;
            if (gap > 0 && (int)card.cardValue > (int)sorted[i].cardValue && (int)card.cardValue < (int)sorted[i + 1].cardValue)
            {
                anchor = sorted[i];
                return true;
            }
        }

        // If no gap is found, allow the card to be added at the end
        anchor = cardsInSequence.Last();
        return true;
    }

    private bool CanFitPureRun(Card card, out Card anchor)
    {
        anchor = null;
        // Pure run = no jokers, strictly consecutive, same suit
        if (card.isJoker) return false;
        if (cardsInSequence.Any(c => c.isJoker)) return false;

        // Check suit
        var sorted = cardsInSequence.OrderBy(c => c.cardValue).ToList();
        if (sorted.Count > 0)
        {
            var suit = sorted[0].cardColor;
            if (card.cardColor != suit) return false;
        }

        // Possibly check if card rank is exactly one above or below the existing min/max
        anchor = sorted.Last();
        return true;
    }

    private bool NeedsToGoBefore(Card newCard, Card anchorCard)
    {
        // If you're forming runs, you might want 
        // newCard with a smaller rank to come before anchorCard, etc.
        return (!newCard.isJoker) && (newCard.cardValue < anchorCard.cardValue);
    }

    // -------------------------------------
    //      PRIVATE HELPER: VALIDATION
    // -------------------------------------

    private bool ValidatePureRun()
    {
        // At least 3 cards, no jokers, strictly consecutive same suit
        if (cardsInSequence.Count < 3) return false;
        if (cardsInSequence.Any(c => c.isJoker)) return false;

        var sorted = cardsInSequence.OrderBy(c => c.cardValue).ToList();
        var suit = sorted[0].cardColor;

        for (int i = 0; i < sorted.Count - 1; i++)
        {
            if (sorted[i].cardColor != suit) return false;
            if ((int)sorted[i + 1].cardValue != (int)sorted[i].cardValue + 1) return false;
        }
        return true;
    }

    private bool ValidateImpureRun()
    {
        // 3+ cards, same color, jokers can fill rank gaps
        if (cardsInSequence.Count < 3) return false;

        var realCards = cardsInSequence.Where(c => !c.isJoker).OrderBy(c => c.cardValue).ToList();
        int jokerCount = cardsInSequence.Count(c => c.isJoker);

        if (realCards.Count == 0) return false; // can't be all jokers

        // All real cards same suit
        var suit = realCards[0].cardColor;
        if (!realCards.All(rc => rc.cardColor == suit)) return false;

        // Check consecutive ignoring how many jokers you might have
        int missingCards = 0;
        for (int i = 1; i < realCards.Count; i++)
        {
            int gap = (int)realCards[i].cardValue - (int)realCards[i - 1].cardValue - 1;
            if (gap > 0)
                missingCards += gap;
        }
        return (missingCards <= jokerCount);
    }
    /// <summary>
    /// Validate a "set," "trail," or "pair" by checking min/max count 
    /// and whether the non-jokers share the same rank.
    /// </summary>
    private bool ValidateSet(int minCount, int maxCount = 100)
    {
        int count = cardsInSequence.Count;
        if (count < minCount || count > maxCount) return false;

        // Must have at least 1 real card
        var nonJokers = cardsInSequence.Where(c => !c.isJoker).ToList();
        if (nonJokers.Count == 0) return false;

        // All non-jokers must share the same rank
        var firstValue = nonJokers[0].cardValue;
        bool allSame = nonJokers.All(c => c.cardValue == firstValue);
        return allSame;
    }
}
