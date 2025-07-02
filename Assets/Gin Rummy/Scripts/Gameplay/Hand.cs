using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

// Extend your existing TypeOfSequence enum *without* Pair, e.g.:
// public enum TypeOfSequence { Run, Set, PureRun, ImpureRun, Trail /* no Pair */ }

public class Hand : DropZone, IResettable
{
    public Action OnCardsPositionRefreshCB;
    public bool isBot;
    private Player _playerOfThisHand;
    private SortingMethod sortingMethod = SortingMethod.Values;

    private float handWidth;
    private float scaleFactor;
    private RectTransform rectTransform;
    private List<CardsSequence> sequencesList = new List<CardsSequence>();
    private Deadwood deadwood;
    protected float cardFitSpeed;

    float spacePerCard;
    float twistPerCard;
    float startTwist;
    bool gameIsStarted;

    SequenceHorizontalPosition sequenceHorizontalPosition = SequenceHorizontalPosition.Right;
    SequnceVerticalPosition sequenceVerticalPosition = SequnceVerticalPosition.Down;

    public Player playerOfThisHand
    {
        get { return _playerOfThisHand; }
        set
        {
            _playerOfThisHand = value;
            _playerOfThisHand.AssignHand(this);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        cardFitSpeed = Constants.QUICK_ANIM_TIME;
        RegisterResetable();
        deck.OnCardDealtCB += UnlockHand;
    }

    protected void Start()
    {
        deadwood = GetComponent<Deadwood>();
        GameManager.OnGameFinishedCB += LockHand;
        rectTransform = transform as RectTransform;
        handWidth = rectTransform.sizeDelta.x;
        ResetState();
    }

    public void RegisterResetable()
    {
        gameManager.RegisterResettable(this);
    }

    public void ResetState()
    {
        LockHand();
        sequencesList.KillSequences();
        CalculateSpaceAndRotationParameters(Constants.CARDS_IN_START_HAND);
    }

    public Card GetCardFromHand(string cardCode)

    {
      
         List<Card> handCards = GetCardsFromZone(); // or however you track the cards
        Card foundCard = handCards.FirstOrDefault(c => c.cardCode == cardCode);

        if (foundCard != null)
        {
            Console.WriteLine("Found card with code: " + foundCard.cardCode);
        }
        else
        {
            Console.WriteLine("No card found with code: " + cardCode);
        }

        return foundCard;
    }

    private void LockHand()
    {
        gameIsStarted = false;
        LockZone();
    }

    private void UnlockHand()
    {
        gameIsStarted = true;
        UnlockZone();
        SortCards();
        AutomaticFindSequences();
    }

    private void CalculateSpaceAndRotationParameters(int startSize = 0)
    {
        int sequencedCards = GetSequencedCards().Count;
        int normalCards = startSize == 0 ? transform.childCount - sequencedCards : startSize;
        float childrenCount = Mathf.Max(1, sequencedCards * GetPositionFactor(true) + normalCards);
        spacePerCard = handWidth / childrenCount;
        twistPerCard = GetFanAngle() / childrenCount;
        startTwist = GetFanAngle() / 2f;
    }

    private float GetFanAngle()
    {
        return 30; // max degree
    }

    public override void AssignNewChild(Transform child, int cardOrder)
    {
        base.AssignNewChild(child);
        child.SetSiblingIndex(cardOrder);
        AutomaticFindSequences();
    }

    public override void AssignNewChild(Transform child)
    {
        int pos = GetLastAvailableBeforeSequencesPosition();
        AssignNewChild(child, pos);
        SortCards();
        ShowCard(child);
    }

    protected virtual void ShowCard(Transform child)
    {
        Card card = child.GetComponent<Card>();
        if (card != null)
        {
            card.isReversed = isBot; // If it's bot, face-down
        }
    }

    public override void UnparentChild(Transform child, Transform newParent)
    {
        base.UnparentChild(child, newParent);
        RegisterReorderAction(child);
        AutomaticFindSequences();
    }

    private void RegisterReorderAction(Transform child)
    {
        Card c = child.GetComponent<Card>();
        if (c != null)
            c.OnPlaceHolderChangePos += ReorderChildren;
    }

    private void UnregisterReorderAction(Transform child)
    {
        Card c = child.GetComponent<Card>();
        if (c != null)
            c.OnPlaceHolderChangePos -= ReorderChildren;
    }

    public List<Card> GetNotSequencedCards()
    {
        List<Card> cards = GetCardsFromZone();
        return cards.Where(x => x.inSequence == null).OrderByDescending(x => x.cardValue).ToList();
    }

    public List<Card> GetSequencedCards()
    {
        List<Card> cards = GetCardsFromZone();
        return cards.Where(x => x.inSequence != null).ToList();
    }

    public List<CardsSequence> GetTotalSequence()
    {
        return sequencesList;
    }

    public void ReorderChildren()
    {
        CalculateSpaceAndRotationParameters();

        int totalSequences = GetTotalSequence().Count;
        int totalCards = GetCardsFromZone().Count;

        float overlapFactor = Constants.CARDS_OVERLAP_FACTOR;

        float spacePerCard = Constants.cardSize.x * (1 - overlapFactor);
        float sequenceSpacing = Constants.cardSize.x + Constants.SEQUENCE_VERTICAL_OFFSET;
        float cardXPos = 0;

        float totalWidth = (totalCards - 1) * spacePerCard
                         + (totalSequences * (sequenceSpacing - Constants.cardSize.x)
                         + totalSequences * (Constants.cardSize.x * overlapFactor));
        float startX = -totalWidth / 2;
        cardXPos = startX;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform cardChild = transform.GetChild(i);
            bool isSequencedCard = IsThisCardIsInAnySequence(cardChild);

            scaleFactor = GetPositionFactor(isSequencedCard);

            if (isSequencedCard && CardIndexInSequence(cardChild) == 0)
            {
                cardXPos += sequenceSpacing;
            }
            else
            {
                if (i > 0)
                {
                    cardXPos += spacePerCard;
                }
            }

            Vector3 pos = new Vector3(cardXPos, 0, 0);
            cardChild.AnimateLocalPosition(pos, cardFitSpeed);
        }

        OnCardsPositionRefreshCB?.Invoke();
    }

    protected virtual float GetScaleFactor(bool isSequencedCard)
    {
        if (isSequencedCard)
            return Constants.SEQUENCED_CARDS_SCALE_FACTOR;
        return 1;
    }

    protected virtual float GetPositionFactor(bool isSequencedCard)
    {
        if (isSequencedCard)
            return Constants.SEQUENCED_POSITION_FACTOR;
        return 1;
    }

    protected virtual float GetVerticalOffset(bool isSequencedCard)
    {
        if ((isSequencedCard && sequenceVerticalPosition == SequnceVerticalPosition.Down))
            return Constants.SEQUENCE_VERTICAL_OFFSET;

        if (isSequencedCard && sequenceVerticalPosition == SequnceVerticalPosition.Up)
        {
            if (sequenceHorizontalPosition == SequenceHorizontalPosition.Left)
                return -Constants.SEQUENCE_VERTICAL_OFFSET * 2f;

            return -Constants.SEQUENCE_VERTICAL_OFFSET * 1.5f;
        }
        return 0;
    }

    private bool IsThisCardIsInAnySequence(Transform cardTransform)
    {
        Card card = cardTransform.GetComponent<Card>();
        return card != null && card.inSequence != null;
    }

    private int CardIndexInSequence(Transform cardTransform)
    {
        Card card = cardTransform.GetComponent<Card>();
        return card != null ? card.inSequence.GetIndexOfCardInSeq(card) : -1;
    }

    public Vector3 GetNextAvailablePosition(out float cardRotation)
    {
        int cardAvailablePlace = GetLastAvailableBeforeSequencesPosition();
        cardRotation = 0; // no rotation in your version

        float overlapFactor = 0.3f;
        float spacePerCard = Constants.cardSize.x * (1 - overlapFactor);
        float totalWidth = (Constants.CARDS_IN_START_HAND - 1) * spacePerCard;
        float startX = -totalWidth / 2;
        float cardXPos = startX + (cardAvailablePlace * spacePerCard);
        Vector3 pos = new Vector3(cardXPos, 0, 0);

        return transform.TransformPoint(pos);
    }

    protected virtual int GetLastAvailableBeforeSequencesPosition()
    {
        if (sequencesList.Count == 0)
            return transform.childCount;

        List<Card> sequencedCards = GetSequencedCards();
        Card card = (sequenceHorizontalPosition == SequenceHorizontalPosition.Right)
                    ? sequencedCards.First()
                    : sequencedCards.Last();
        return card.transform.GetSiblingIndex();
    }

    public override void OnDrop(PointerEventData eventData)
    {
        // If needed, override your drop logic here
    }

    public void ChangeSortingType(SortingMethod method)
    {
        sortingMethod = method;
        SortCards();
    }

    private void SortCards()
    {
        if (gameIsStarted)
        {
            switch (sortingMethod)
            {
                case SortingMethod.Values:
                    SortCardsByValue();
                    break;
                case SortingMethod.Colors:
                    SortCardsByColor();
                    break;
                default:
                    break;
            }
        }
    }

    private void SortCardsByValue()
    {
        List<Card> cards = SortSequences(TypeOfSequence.Set);
        List<Card> cardsInHand = GetCardsFromZone().SortCardsByValue();
        cards.InsertUniqueValues(cardsInHand);
        SyncAndReorderCards(cards);
    }

    private void SortCardsByColor()
    {
        List<Card> cards = SortSequences(TypeOfSequence.Run);
        List<Card> cardsInHand = GetCardsFromZone().SortCardsByColor();
        cards.InsertUniqueValues(cardsInHand);
        SyncAndReorderCards(cards);
    }

    private List<Card> SortSequences(TypeOfSequence sortType)
    {
        List<Card> cards = new List<Card>();
        // reorder sequences so that the type we want is first
        sequencesList = sequencesList.OrderByDescending(x => x.sequenceType == sortType).ToList();
        for (int i = 0; i < sequencesList.Count; i++)
        {
            cards.AddRange(sequencesList[i].GetCardsOfThisSequence());
        }
        return cards;
    }

    private void SyncAndReorderCards(List<Card> cards)
    {
        SyncCardPosition(cards);
        AutomaticFindSequences();
    }

    void SyncCardPosition(List<Card> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.SetAsLastSibling();
        }
    }

    public void AutomaticFindSequences()
    {
        if (!gameIsStarted) return;

        // Clear existing sequences
        sequencesList.KillSequences();

        List<Card> cards = GetCardsFromZone();

        // Try "Color-first" approach
        int firstByColorSequencesScore = FindSequencesByColorFirst(cards);

        // Reset and try "Value-first" approach
        sequencesList.KillSequences();
        int firstByValueSequencesScore = FindSequencesByValueFirst(cards);

        // Compare which approach yields better score
        sequencesList.KillSequences();
        if (firstByColorSequencesScore >= firstByValueSequencesScore)
        {
            FindSequencesByColorFirst(cards);
        }
        else
        {
            FindSequencesByValueFirst(cards);
        }

        // Finally reorder the deck by found sequences
        SortDeckBySequences();
    }

    public int GetSequencesScore()
    {
        return sequencesList.GetSequencesScore();
    }

    public int FindSequencesByColorFirst(List<Card> cards)
    {
        sequencesList.KillSequences();
        cards = cards.SortCardsByColor();
        SearchForRunSequences(cards); // find run or run variants
        cards = cards.SortCardsByValue();
        SearchForSetSequences(cards); // find set/trail
        return sequencesList.GetSequencesScore();
    }

    public int FindSequencesByValueFirst(List<Card> cards)
    {
        sequencesList.KillSequences();
        cards = cards.SortCardsByValue();
        SearchForSetSequences(cards);
        cards = cards.SortCardsByColor();
        SearchForRunSequences(cards);
        return sequencesList.GetSequencesScore();
    }

    private void SortDeckBySequences()
    {
        List<Card> cards = new List<Card>();

        // gather sequenced cards
        for (int i = 0; i < sequencesList.Count; i++)
        {
            cards.AddRange(sequencesList[i].GetCardsOfThisSequence());
        }

        // gather leftover cards
        List<Card> cardsInHand = GetCardsFromZone();
        if (sequenceHorizontalPosition == SequenceHorizontalPosition.Right)
            cards.InsertUniqueValues(cardsInHand);
        else
            cards.AddUniqueValues(cardsInHand);

        SyncCardPosition(cards);
        ReorderChildren();
    }

    // --------------------------------------------------------------------
    //  NEW / EXTENDED LOGIC FOR PURE RUN, IMPURE RUN, TRAIL, ETC. (NO PAIR)
    // --------------------------------------------------------------------

    delegate bool SequenceValidationCondition(Card prevCard, Card card);

    private void SearchForRunSequences(List<Card> cards)
    {
        SequenceValidationCondition cond = IsValidElementOfRunSequence;
        SearchForSequences(cond, cards, TypeOfSequence.Run);
    }
    private void SearchForSetSequences(List<Card> cards)
    {
        SequenceValidationCondition cond = IsValidElementOfSetSequence;
        SearchForSequences(cond, cards, TypeOfSequence.Set);
    }

    private void SearchForSequences(SequenceValidationCondition sequenceValidationCondition,
                                List<Card> cards,
                                TypeOfSequence searchedType)
    {
        List<Card> possibleSequence = new List<Card>();
        List<Card> cardsToRemove = new List<Card>();

        for (int i = 1; i < cards.Count; i++)
        {
            Card prevCard = cards[i - 1];
            Card card = cards[i];

            if (sequenceValidationCondition(prevCard, card) || prevCard.isJoker || card.isJoker)
            {
                possibleSequence.AddUniqueValue(prevCard);
                possibleSequence.AddUniqueValue(card);
            }
            else
            {
                TryAddNewCardsToRemove(possibleSequence, cardsToRemove, searchedType);
                possibleSequence.Clear();
            }
        }
        TryAddNewCardsToRemove(possibleSequence, cardsToRemove, searchedType);

        // Remove from the source list
        cards.RemoveCardsFromList(cardsToRemove);
    }


    private void TryAddNewCardsToRemove(List<Card> possibleSequence,
                                     List<Card> cardsToRemove,
                                     TypeOfSequence fallbackType)
    {
        if (possibleSequence.Count == 0) return;

        // Remove duplicates in case they've been used already
        RemoveDuplicateCardsFromSequence(possibleSequence);

        // Decide if it's a valid sequence and which type it is
        TypeOfSequence determinedType = DetermineSequenceType(possibleSequence, fallbackType);
        // NO 'Pair' in your TypeOfSequence. We only allow Run, Set, PureRun, ImpureRun, Trail.
        if (determinedType != TypeOfSequence.Run
            && determinedType != TypeOfSequence.Set
            && determinedType != TypeOfSequence.PureRun
            && determinedType != TypeOfSequence.ImpureRun
            && determinedType != TypeOfSequence.Trail)
        {
            // Not valid
            return;
        }

        // Only form a new sequence if we meet the minimum cards for that type
        if (possibleSequence.Count >= Constants.MIN_NUMBER_OF_CARDS_IN_SEQUENCE)
        {
            CardsSequence sequence = new CardsSequence(
                possibleSequence,
                // If we have fewer colors than sequences, clamp or cycle the index:
                Constants.sequenceColors[sequencesList.Count % Constants.sequenceColors.Length],
                determinedType
            );
            sequencesList.Add(sequence);
            cardsToRemove.AddRange(possibleSequence);
        }
    }

    private TypeOfSequence DetermineSequenceType(List<Card> sequenceCandidate, TypeOfSequence fallbackType)
    {
        if (IsPureRun(sequenceCandidate)) return TypeOfSequence.PureRun;
        if (IsImpureRun(sequenceCandidate)) return TypeOfSequence.ImpureRun;
        if (IsTrail(sequenceCandidate)) return TypeOfSequence.Trail;

        return fallbackType;
    }

    private bool IsPureRun(List<Card> possibleSequence)
    {
        if (possibleSequence.Count < 3) return false;
        if (possibleSequence.Any(c => c.isJoker)) return false;

        var sorted = possibleSequence.OrderBy(c => c.cardValue).ToList();
        for (int i = 0; i < sorted.Count - 1; i++)
        {
            if (sorted[i].cardColor != sorted[i + 1].cardColor) return false;
            if ((int)sorted[i + 1].cardValue != (int)sorted[i].cardValue + 1)
                return false;
        }
        return true;
    }


    private bool IsImpureRun(List<Card> possibleSequence)
    {
        if (possibleSequence.Count < 3) return false;

        var nonJoker = possibleSequence.Where(c => !c.isJoker).ToList();
        if (nonJoker.Count == 0) return false; // Must contain at least one real card

        var suit = nonJoker[0].cardColor;
        if (!nonJoker.All(c => c.cardColor == suit)) return false; // Same color required

        nonJoker = nonJoker.OrderBy(c => c.cardValue).ToList();
        for (int i = 0; i < nonJoker.Count - 1; i++)
        {
            int diff = (int)nonJoker[i + 1].cardValue - (int)nonJoker[i].cardValue;
            if (diff > 2) return false; // Allow up to 1 missing card (filled by Joker)
        }
        return true;
    }

    private bool IsTrail(List<Card> possibleSequence)
    {
        // 3 or 4 of a kind
        if (possibleSequence.Count < 3 || possibleSequence.Count > 4) return false;

        var nonJokerCards = possibleSequence.Where(c => !c.isJoker).ToList();
        if (nonJokerCards.Count == 0) return false;

        var firstValue = nonJokerCards[0].cardValue;
        bool allSame = nonJokerCards.All(c => c.cardValue == firstValue);
        return allSame;
    }

    private void RemoveDuplicateCardsFromSequence(List<Card> possibleSequence)
    {
        for (int i = possibleSequence.Count - 1; i >= 0; i--)
        {
            Card cardToCheck = possibleSequence[i];
            for (int j = 0; j < sequencesList.Count; j++)
            {
                if (sequencesList[j].HasThisCardInSequence(cardToCheck))
                {
                    possibleSequence.RemoveAt(i);
                    break;
                }
            }
        }
    }

    // Legacy methods for 'Run' and 'Set' fallback checks
    public bool IsValidElementOfRunSequence(Card prevCard, Card card)
    {
        if (prevCard.isJoker || card.isJoker) return true; // Allow Joker as a replacement

        bool sameColor = prevCard.cardColor == card.cardColor;
        bool correctOrder = (int)prevCard.cardValue + 1 == (int)card.cardValue;

        return sameColor && correctOrder;
    }

    public bool IsValidElementOfSetSequence(Card prevCard, Card card)
    {
        return CardsHaveSameValues(prevCard, card) || prevCard.isJoker || card.isJoker;
    }

    private bool CardAreTheSameColor(Card prevCard, Card card)
    {
        return prevCard.cardColor == card.cardColor
               || prevCard.isJoker
               || card.isJoker;
    }

    private bool CardsAreInCorrectOrder(Card prevCard, Card card)
    {
        return ((int)prevCard.cardValue + 1 == (int)card.cardValue)
               || prevCard.isJoker
               || card.isJoker;
    }

    private bool CardsHaveSameValues(Card prevCard, Card card)
    {
        return prevCard.cardValue == card.cardValue;
    }

    public bool IsValidHand()
    {
        // Example Rummy check: at least 2 sequences, 1 must be pure run
        int totalSequenceCount = sequencesList.Count;
        int pureSequenceCount = sequencesList.Count(seq => seq.sequenceType == TypeOfSequence.PureRun);

        return (totalSequenceCount >= 2 && pureSequenceCount >= 1);
    }

    public bool CanWinThisGame()
    {
        return deadwood.PlayerCanWinNow();
    }

    public int GetDeadwoodScore()
    {
        return deadwood.GetDeadwoodScore();
    }

    public void RunDeadwoodScoreAnimation(Action onCompleteCallback)
    {
        LockZone();
        deadwood.OnDeadwoodAnimationFinishedCB = null;
        deadwood.OnDeadwoodAnimationFinishedCB += onCompleteCallback;
        deadwood.OnDeadwoodAnimationFinishedCB += UnlockZone;
        deadwood.RunDeadwoodPointsAnimation();
    }

    public void StopDeadwoodScoreAnimation()
    {
        deadwood.StopDeadwoodScoreAnimation();
        UnlockZone();
    }
}
