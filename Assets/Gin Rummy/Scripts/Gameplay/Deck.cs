using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;
using SecPlayerPrefs;
using System.Reflection;

public class Deck : MonoBehaviour, ITakeCard, IPointerDownHandler
{
    [SerializeField] GameObject[] cardPrefabs;
    [SerializeField] List<Card> cardsPool;
    private DiscardPile discardPile;
    private GameManager gameManager;
    [SerializeField] bool lockTakeCard;
    private Text cardsOnDeckText;

    public Action OnCardDealtCB;

    public CardData cachedCard;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        cardPrefabs = Resources.LoadAll<GameObject>("CardPrefabs/");
        cardsOnDeckText = transform.parent.GetComponentInChildren<Text>();
        discardPile = FindObjectOfType<DiscardPile>();
        PrepareObjectsPool();
    }



    public void OnReshuffleCard(PlayerDeck data)
    {


    }
    private void Start()
    {
        TurnOffDeck();
    }

    public void TurnOffDeck()
    {
        gameObject.SetActive(false);
    }
    public void TurnOnDeck()
    {
        gameObject.SetActive(true);
    }

    public void PrepareObjectsPool()
    {

        cardsPool = new List<Card>();

        // Load all card prefabs including Jokers
        for (int i = 0; i < cardPrefabs.Length; i++)
        {
            Card card = Instantiate(cardPrefabs[i], transform).GetComponent<Card>();
            cardsPool.Add(card);
        }

        // 🔴 Add Joker Cards to the Deck
        AddJokerCards();

    }// 🃏 Function to Ensure Jokers Exist in the Deck
    private void AddJokerCards()
    {
        int redJokerCount = cardsPool.Count(c => c.cardColor == CardColor.RED && (int)c.cardValue == 0);
        int blackJokerCount = cardsPool.Count(c => c.cardColor == CardColor.BLACK && (int)c.cardValue == 0);

        if (redJokerCount == 0)
        {
            AddJokerToPool("52JokerRed", CardColor.RED);
        }

        if (blackJokerCount == 0)
        {
            AddJokerToPool("53JokerBlack", CardColor.BLACK);
        }
    }
    // 🃏 Helper Function to Add Jokers
    private void AddJokerToPool(string prefabName, CardColor suit)
    {
        GameObject jokerPrefab = Resources.Load<GameObject>($"CardPrefabs/{prefabName}");
        if (jokerPrefab != null)
        {
            Card jokerCard = Instantiate(jokerPrefab, transform).GetComponent<Card>();
            jokerCard.cardColor = suit;
            jokerCard.cardValue = (CardValue)0; // Joker value is 0
            cardsPool.Add(jokerCard);
        }
        else
        {
            Debug.LogError($"[AddJokerToPool] Joker prefab {prefabName} not found in Resources!");
        }
    }
    public void PrepareDeckToReorder(int gameRandomSeed, Hand[] handsToDeal)
    {
        List<Card> cards = GetCardsFromDeck();
        cards = ShuffleFromSeed(gameRandomSeed, cards);
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.SetAsLastSibling();
        }
        //BackCardsToInitialState();
        DealCards(handsToDeal, cards);// not working
    }

    public void ResetState(int randomSeed)
    {
        cardsPool = ShuffleFromSeed(randomSeed, cardsPool);
        BackCardsToInitialState();

    }
    public void ResetStateCardsPool()
    {
        foreach (Card card in cardsPool)
        {
            Destroy(card);
        }

    }


    private void ShuffleCards()
    {
        cardsPool.ShuffleList();
        Resources.UnloadUnusedAssets();
    }

    private List<Card> ShuffleFromSeed(int seed, List<Card> elementsToSort)
    {
        int[] myRandomNumbers = Randomizer.GetRandomDeckFromSeed(seed, elementsToSort.Count);
        return SortCardsByIDArray(myRandomNumbers, elementsToSort);
    }

    private List<Card> SortCardsByIDArray(int[] idsArray, List<Card> elementsToSort)
    {
        if (cardsPool == null)
            PrepareObjectsPool();
        try
        {
            elementsToSort = elementsToSort.Where(card => card != null).OrderBy(x => x.name).ToList();

            //elementsToSort = elementsToSort.OrderBy(x => x.name).ToList();
            List<Card> tempCards = new List<Card>();
            for (int i = 0; i < elementsToSort.Count; i++)
            {
                tempCards.Add(elementsToSort[idsArray[i]]);
            }
            return tempCards;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return elementsToSort;
        }

    }



    private void BackCardsToInitialState()
    {
        if (cardsPool == null)
            PrepareObjectsPool();

        for (int i = 0; i < cardsPool.Count; i++)
        {
            if (cardsPool[i] == null)
            {
                Debug.LogException(new Exception("'BackCardsToInitialState' - Null card in card pool! " + i));
                continue;
            }
            cardsPool[i].isReversed = true;
            cardsPool[i].transform.SetParent(transform);
            cardsPool[i].transform.localPosition = Vector3.zero;
            cardsPool[i].transform.localScale = Constants.vectorOne;
            cardsPool[i].transform.rotation = transform.rotation;
            cardsPool[i].transform.SetAsLastSibling();
        }
        Resources.UnloadUnusedAssets();
    }

    public void DealCards(Hand[] handsToDeal, List<Card> deck = null)
    {
        if (deck == null)
            deck = cardsPool;
        gameObject.SetActive(true);
        RefreshCardsCountText();
        StartCoroutine(DealCardsCoroutine(handsToDeal, deck));


    }


    private IEnumerator DealCardsCoroutine(Hand[] handsToDeal, List<Card> deck)
    {
        int index = 0;
        yield return new WaitForSeconds(Constants.TIME_BETWEEN_DEAL_CARD);

        for (int i = 0; i < GameManager.instance.playerCardsReponce.players[0].cards.Count; i++)
        {
            for (int j = 0; j < handsToDeal.Length; j++)
            {
                Hand hand = handsToDeal[j];
                float cardRotationAngle = 0;
                Vector3 moveDestination = hand.GetNextAvailablePosition(out cardRotationAngle);
                CardDestination cardDestination = new CardDestination(hand.transform, moveDestination, 0);
                var playerCardData = GameManager.instance.playerCardsReponce.players.Find(p => p.playerId == hand.playerOfThisHand.playerId);
                var cardData = playerCardData.cards[i];
                Card card = GetCardFromData(cardData, deck);

                card.DealCard(cardDestination);
                card.isReversed = true;
                index++;
                RefreshCardsCountText();
                yield return new WaitForSeconds(Constants.TIME_BETWEEN_DEAL_CARD);
            }

            if (handsToDeal.Length <= 1)
                yield return new WaitForSeconds(Constants.TIME_BETWEEN_DEAL_CARD);
        }
        PutLastCardToDiscardPile(GetCardFromData(GameManager.instance.playerCardsReponce.lastCard, deck));
        RefreshCardsCountText();
        yield return new WaitForSeconds(Constants.DEAL_ANIM_TIME);
        OnCardDealtCB.RunAction();
    }


    private Card GetCardFromData(CardData cardData, List<Card> deck)
    {
        Card card = deck.Find(c =>
        {
            // Check for Joker case first
            if (cardData.suit.Equals("RED", StringComparison.OrdinalIgnoreCase) && cardData.value == "0")
            {
                return c.cardColor.ToString().Equals("RED", StringComparison.OrdinalIgnoreCase) && (int)c.cardValue == 0;
            }
            if (cardData.suit.Equals("BLACK", StringComparison.OrdinalIgnoreCase) && cardData.value == "0")
            {
                return c.cardColor.ToString().Equals("BLACK", StringComparison.OrdinalIgnoreCase) && (int)c.cardValue == 0;
            }

            // Normal card check based on suit
            if (!c.cardColor.ToString().Equals(cardData.suit, StringComparison.OrdinalIgnoreCase))
                return false;

            // Try to parse cardData.value as an integer (for numbered cards)
            if (int.TryParse(cardData.value, out int cardValueInt))
            {
                return (int)c.cardValue == cardValueInt;
            }
            else
            {
                // Named card values (e.g., "ACE", "QUEEN", "KING")
                if (Enum.TryParse<CardValue>(cardData.value, true, out CardValue cardValueEnum))
                {
                    return c.cardValue == cardValueEnum;
                }
                else
                {
                    return false;
                }
            }
        });

        return card;
    }
    //private void PutLastCardToDiscardPile(int index, List<Card> deck)
    //{
    //    CardDestination cardDestination = new CardDestination(discardPile.transform, discardPile.transform.position, 0);
    //    Card card = deck[index].GetComponent<Card>();
    //    card.DealCard(cardDestination);
    //}
    private void PutLastCardToDiscardPile(Card card)
    {
        CardDestination cardDestination = new CardDestination(discardPile.transform, discardPile.transform.position, 0);
        card.DealCard(cardDestination);
    }

    public Card TakeCard(Hand hand)
    {

        Card card = transform.TakeCard(hand);
        RefreshCardsCountText();
        CheckIfDeckIsEmpty();
        if (card == null)
        {
            Debug.Log("Deck is empty! Reorder it!");
        }
        else
        {
            card.NormalColorCard();
        }


        return card;
    }

    private void CheckIfDeckIsEmpty()
    {
        int remainingOnDeckCards = GetRemainingCardsNumber();
        if (remainingOnDeckCards == 0)
        {
            Debug.LogError($"Deck is Empty");
            // ReturnDiscardedCardsToDeck();
        }

    }

    private void ReturnDiscardedCardsToDeck()
    {
        Card[] cards = discardPile.GetComponentsInChildren<Card>();

        cards.ShuffleArrayFromSeed(gameManager.gameData.gameRandomSeed);
        for (int i = 0; i < cards.Length; i++)
        {
            Card card = cards[i];
            CardDestination cardDestination = new CardDestination(transform, transform.position, 0);
            card.DealCard(cardDestination);
            card.RotateCard(Constants.DEAL_ANIM_TIME / 2);
            if (i == cards.Length - 1)
                RegisterCallbackOnLastCardInDeck(card);
        }
    }

    private void RegisterCallbackOnLastCardInDeck(Card card)
    {
        card.OnReverseAnimationFinishCB = () =>
        {
            RefreshCardsCountText();
            card.OnReverseAnimationFinishCB = null;
        };
    }

    private int GetRemainingCardsNumber()
    {
        return transform.GetComponentsInChildren<Card>().Length;
    }

    public async void OnPointerDown(PointerEventData eventData)
    {
        if (lockTakeCard)
            return;
        if (gameManager.IsValidTimeToTakeCard() || gameManager.IsTakeFromStockPilePhase())
        {
            gameManager.currentPlayer.TakeCard(this);


            RunLockCooldown();
            discardPile.RunLockCooldown();
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

    public void RefreshCardsCountText()
    {
        string cardAmountTxt = transform.childCount > 0 ? transform.childCount.ToString() : "";
        cardsOnDeckText.text = cardAmountTxt;
    }

    public void FadeDeckCards(bool fade)
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
        transform.parent.DOShakeScale(1, 0.12f, 3, 45);
    }


    public List<Card> GetCardsFromDeck()
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


    public void SetCardOnTop(FetchedCardData cardData)
    {
        int index = 0;
        // Dynamically find BotHand in the Canvas UI
        BotHand botHand = GameObject.FindObjectOfType<BotHand>();
        for (int i = 0; i < GameManager.instance.playerCardsReponce.players.Count; i++)
        {

            if (GameManager.instance.playerCardsReponce.players[i].playerId == cardData.requestedBy)
            {
                for (int j = this.transform.childCount - 1; j >= 0; j--)
                {
                    Card card = transform.GetChild(j).GetComponent<Card>();
                    if (card != null && cardData.card.code == card.cardCode)
                    {

                        Debug.Log($"Moving Card: {card.cardCode} to BotHand");

                        // Set card's new parent as BotHand
                        card.transform.SetParent(botHand.transform, false);

                        // Reset card's position & rotation for correct alignment
                        card.transform.localPosition = Vector3.zero;
                        card.transform.localRotation = Quaternion.identity;

                        // Ensure proper ordering in BotHand
                        card.transform.SetAsFirstSibling();

                        ////Destroy(card.gameObject);
                        //// card.gameObject.SetActive(false);
                        //card.transform.SetAsFirstSibling();

                    }
                }

            }


        }


    }

}
