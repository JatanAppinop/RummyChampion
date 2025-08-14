using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;
using SecPlayerPrefs;

[Serializable]
public class Player
{
    //"67ab2b01dcef4cbe25d56bb4"
    public string playerId;
    public Action OnFinishMoveCB;

    public UserData userData;
    private int totalWinnings = 0; // New variable to store total winnings
    
    // 🔹 NEW DROP AND GAME STATE PROPERTIES
    [field: SerializeField] public bool hasDropped { get; private set; } = false;
    [field: SerializeField] public bool isEliminated { get; private set; } = false;
    [field: SerializeField] public int cumulativeScore { get; private set; } = 0;
    [field: SerializeField] public int dealsWon { get; private set; } = 0;
    [field: SerializeField] public bool hasPickedCardThisTurn { get; private set; } = false;
    public Action<Player> OnPlayerDropped;
    public Action<Player> OnPlayerEliminated;

    [field: SerializeField] private string _name;


    public string name
    {
        get { return _name; }
        set
        {
            _name = value;
            playerUI.RefreshPlayerName(name);
        }
    }
    bool gameFinishedCalled=false;
    public bool gameFinished
    {
        get { return gameFinishedCalled; }
        set
        {
            gameFinishedCalled = value;
            
        }
    }
    public int avatarID { get; protected set; }

    protected Hand myHand;
    protected Deck deck;
    protected DiscardPile discardPile;
    protected GameManager gameManager;

    private Card _cardOnAction;
    private bool lastCardIsTakenFromDeck;
    public PlayerUI playerUI; // 🔹 CHANGED: Made public to allow external access
    private bool inAction;
    private Card zoomedCard;
    public Card GetZoomedCard => zoomedCard;

    protected Card cardOnAction
    {

        get
        {
            if (_cardOnAction == null)
            {
                Debug.LogError("cardOnAction is NULL when accessed!");
            }
            return _cardOnAction;
        }
        set
        {
            if (value == null)
            {
                Debug.LogWarning("Assigning NULL to cardOnAction!");
            }
            _cardOnAction = value;
            inAction = _cardOnAction != null;
        }

    }

    public Player(Deck deck, string playerUIName = "PlayerUI")
    {
        this.deck = deck;
        discardPile = GameObject.FindObjectOfType<DiscardPile>();
        gameManager = GameObject.FindObjectOfType<GameManager>();
        InitPlayerUI(playerUIName);
    }

    protected virtual void InitPlayerUI(string playerUIName)
    {
        playerUI = GameObject.Find(playerUIName).GetComponent<PlayerUI>();
        playerUI.ResetUI(PerformAction);
        playerUI.RegisterTimerIsEndingAction(gameManager.ShakeValidDeck);
    }

    public void AssignHand(Hand hand)
    {
        myHand = hand;
    }

    public async void GetPlayerData()
    {

        if (playerId == UserDataContext.Instance.UserData._id)
        {
            userData = UserDataContext.Instance.UserData;
        }
        else
        {
            var userResponce = await APIServices.Instance.GetAsync<User>(APIEndpoints.getUser + playerId);
            if (userResponce != null && userResponce.success)
            {
                userData = userResponce.data;
            }
            else
            {
                UnityNativeToastsHelper.ShowShortText(userResponce.message);
                Debug.LogError(userResponce.message);
            }
        }

        this.name = userData.username;
        SetPlayerAvatar(userData.profilePhotoIndex);

    }

    #region Player_Game_Actions
    public virtual void PerformAction()
    {
        if (inAction == false && gameManager.IsGameEnded() == false)
        {
            Action action = gameManager.GetCurrentTurnProperAction(this).RunAction;
            RunTimerWithAction(action);

            if (zoomedCard != null)
                zoomedCard.Unzoom(null);
        }
    }

    private void RunTimerWithAction(Action action)
    {
        myHand.LockZone();
        new Timer(Constants.AUTOTURN_DELAY,
            () =>
            {
                if (gameManager.IsGameEnded() == false)
                    action();
            });
    }

    public virtual void PassOrTake()
    {
        if (Randomizer.GetRandomDecision())
        {
            gameManager.OnPass();
        }
        else
        {
            TakeCard(discardPile);
        }
    }

    public void TakeStockPile()
    {
        TakeCard(deck);
    }

    public void TakeDiscardPile()
    {
        TakeCard(discardPile);
    }

    public virtual void Discard()
    {
        RestoreCardColor();
        cardOnAction = GetRandomNotSequencedCard();
        DiscardSelectedCard();
    }



    public void Discard(string cardCode)
    {
        cardOnAction = myHand.GetCardFromHand(cardCode);

        if (cardOnAction == null)
        {
            Debug.LogError($"[Discard] No card found in hand with code: {cardCode}");
            return;
        }

        RestoreCardColor();
        DiscardSelectedCard();
        Debug.Log($"[Discard] Card Discarded: {cardOnAction.gameObject.name}");
    }


    public void Discard(Card card)
    {
        if (card == null)
        {
            Debug.LogError("Trying to discard a NULL card!");
            return;
        }

        if (!IsValidCardToDiscard(card))
        {
            Debug.LogError($"Card {card.cardCode} is not valid for discard!");
            return;
        }

        Debug.Log($"Card Discarded: {card.cardCode}");
        RestoreCardColor();
        cardOnAction = card;
        DiscardSelectedCard();
    }

    public void DiscardZoomedCard()
    {
        if (zoomedCard != null && zoomedCard.isCardZoomed)
        {
            Discard(zoomedCard);
            zoomedCard = null;
        }
    }

    private void RestoreCardColor()
    {
        if (cardOnAction != null && gameManager.IsThisGamePlayer(this))
            cardOnAction.NormalColorCard();
    }

    protected void DiscardSelectedCard()
    {
        if (cardOnAction == null)
        {
            Debug.LogError("Error: Trying to discard a null card!");
            return;
        }

        Debug.Log($"[DiscardSelectedCard] Discarding card: {cardOnAction.cardCode}");

        myHand.LockZone();
        cardOnAction.transform.DOKill();
        discardPile.OnDrop(cardOnAction);
        cardOnAction.MoveCard(Constants.MOVE_ANIM_TIME);
        cardOnAction.RegisterAction(FinishMove);
        cardOnAction.RegisterAction(myHand.AutomaticFindSequences);
        zoomedCard = null;
    }


    public void TakeCard()
    {
        ITakeCard randomPile = GetPile();
        TakeCard(randomPile);
    }

    public async void TakeCard(ITakeCard cardPile)
    {
        if (cardPile == null)
        {
            Debug.LogError("cardPile is null!");
            return;
        }
        lastCardIsTakenFromDeck = CheckIfCardIsFromDeck(cardPile);
        cardOnAction = cardPile.TakeCard(myHand);

        if (cardOnAction == null)
        {
            Debug.LogError("No card was drawn from pile!");
            return;
        }


        Debug.Log($"[TakeCard] Card Taken: {cardOnAction.cardCode}");

       await  RummySocketServer.Instance.SendEvent(RummySocketEvents.request_card, new Dictionary<string, string>() { { "cardCode", cardOnAction.cardCode },
                                             { "playerId", playerId },{"matchId", SecurePlayerPrefs.GetString(Appinop.Constants.KMatchId)} });

        //await RummySocketServer.Instance.SendEvent(RummySocketEvents.request_card, new Dictionary<string, string>() { { "cardCode", transform.GetFirstAvailableCard().cardCode },
                                            //{ "playerId",UserDataContext.Instance.UserData._id },{"matchId", SecurePlayerPrefs.GetString(Appinop.Constants.KMatchId)} });

        if (gameManager.IsThisGamePlayer(this) && !lastCardIsTakenFromDeck)
            cardOnAction.DisableColorCard();

        // 🔹 Mark that player has picked a card this turn (for drop penalty calculation)
        MarkCardPickedThisTurn();

        cardOnAction.RegisterAction(FinishMove);




    }

    #endregion

    // 🔹 NEW DROP FUNCTIONALITY
    public void DropFromGame(bool hasPickedCard = false)
    {
        if (hasDropped)
        {
            Debug.LogWarning($"Player {name} has already dropped!");
            return;
        }

        hasDropped = true;
        
        // Apply drop penalty based on whether player has picked a card
        int dropPenalty = hasPickedCard || hasPickedCardThisTurn ? Constants.MID_DROP_PENALTY : Constants.FULL_DROP_PENALTY;
        AddToCumulativeScore(dropPenalty);
        
        Debug.Log($"Player {name} dropped with {dropPenalty} penalty points");
        
        // 🔹 SEND DROP EVENT TO SERVER
        SendPlayerDroppedEvent(dropPenalty, hasPickedCard || hasPickedCardThisTurn);
        
        // Notify game manager about the drop
        OnPlayerDropped?.Invoke(this);
        
        // Stop player's timer if it's their turn
        if (gameManager.IsThisPlayerTurn(this))
        {
            StopTimer();
        }
    }

    public void AddToCumulativeScore(int points)
    {
        cumulativeScore += points;
        
        // 🔹 SEND SCORE UPDATE TO SERVER
        SendCumulativeScoreUpdate(points);
        
        // Check for elimination in Pool Rummy
        if (gameManager.gameMode == GameMode.Pool)
        {
            CheckForElimination();
        }
        
        playerUI.UpdateCumulativeScore(cumulativeScore);
    }

    public void CheckForElimination()
    {
        if (isEliminated) return;
        
        int eliminationThreshold = gameManager.GetEliminationThreshold();
        if (cumulativeScore >= eliminationThreshold)
        {
            isEliminated = true;
            Debug.Log($"Player {name} eliminated with {cumulativeScore} points");
            
            // 🔹 SEND ELIMINATION EVENT TO SERVER
            SendPlayerEliminatedEvent();
            
            OnPlayerEliminated?.Invoke(this);
        }
    }

    public void IncrementDealsWon()
    {
        dealsWon++;
    }

    public void ResetForNewDeal()
    {
        hasPickedCardThisTurn = false;
        // Don't reset hasDropped or isEliminated - these persist across deals
    }

    public void MarkCardPickedThisTurn()
    {
        hasPickedCardThisTurn = true;
    }

    public bool CanDrop()
    {
        return !hasDropped && !isEliminated && gameManager.gameMode == GameMode.Pool;
    }

    public int GetFinalScore()
    {
        if (hasDropped)
        {
            // Return the drop penalty as final score
            return hasPickedCardThisTurn ? Constants.MID_DROP_PENALTY : Constants.FULL_DROP_PENALTY;
        }
        
        // Calculate deadwood score from hand
        return myHand != null ? myHand.GetDeadwoodScore() : 0;
    }
    
    // 🔹 PUBLIC METHODS FOR SETTING PROPERTIES WITH PRIVATE SETTERS
    
    public void SetDroppedState(bool dropped)
    {
        hasDropped = dropped;
    }
    
    public void SetEliminatedState(bool eliminated)
    {
        isEliminated = eliminated;
    }
    
    public void SetCumulativeScore(int score)
    {
        cumulativeScore = score;
    }
    
    public void SetDealsWon(int deals)
    {
        dealsWon = deals;
    }
    
    public void SetHasPickedCardThisTurn(bool picked)
    {
        hasPickedCardThisTurn = picked;
    }
    
    // 🔹 NEW SERVER COMMUNICATION METHODS
    
    private async void SendPlayerDroppedEvent(int penaltyPoints, bool hasPickedCard)
    {
        try
        {
            var dropData = new PlayerDroppedData
            {
                playerId = this.playerId,
                playerName = this.name,
                penaltyPoints = penaltyPoints,
                hasPickedCard = hasPickedCard,
                cumulativeScore = this.cumulativeScore,
                gameMode = gameManager.gameMode.ToString(),
                matchId = SecurePlayerPrefs.GetString(Appinop.Constants.KMatchId)
            };
            
            await RummySocketServer.Instance.SendEnhancedEvent(RummySocketEvents.player_dropped, dropData);
            Debug.Log($"Sent player_dropped event for {name}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send player_dropped event: {e.Message}");
        }
    }
    
    private async void SendPlayerEliminatedEvent()
    {
        try
        {
            var eliminationData = new PlayerEliminatedData
            {
                playerId = this.playerId,
                playerName = this.name,
                eliminationScore = this.cumulativeScore,
                threshold = gameManager.GetEliminationThreshold(),
                gameMode = gameManager.gameMode.ToString(),
                matchId = SecurePlayerPrefs.GetString(Appinop.Constants.KMatchId),
                remainingPlayers = gameManager.GetActivePlayersCount()
            };
            
            await RummySocketServer.Instance.SendEnhancedEvent(RummySocketEvents.player_eliminated, eliminationData);
            Debug.Log($"Sent player_eliminated event for {name}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send player_eliminated event: {e.Message}");
        }
    }
    
    private async void SendCumulativeScoreUpdate(int scoreThisRound)
    {
        try
        {
            var scoreData = new CumulativeScoreData
            {
                playerId = this.playerId,
                scoreThisRound = scoreThisRound,
                cumulativeScore = this.cumulativeScore,
                isEliminated = this.isEliminated,
                gameMode = gameManager.gameMode.ToString(),
                matchId = SecurePlayerPrefs.GetString(Appinop.Constants.KMatchId)
            };
            
            await RummySocketServer.Instance.SendEnhancedEvent(RummySocketEvents.cumulative_score_updated, scoreData);
            Debug.Log($"Sent cumulative_score_updated event for {name}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send cumulative_score_updated event: {e.Message}");
        }
    }

    protected virtual ITakeCard GetPile()
    {
        if (Randomizer.GetRandomDecision())
            return deck;
        return discardPile;
    }

    protected virtual void FinishMove()
    {
        if (cardOnAction != null)
        {
            cardOnAction.UnregisterAction(FinishMove);
            cardOnAction.UnregisterAction(myHand.ReorderChildren);
            cardOnAction.UnregisterAction(myHand.AutomaticFindSequences);
        }
        inAction = false;
        UnlockHand();
        OnFinishMoveCB.RunAction();
    }



    private bool CheckIfCardIsFromDeck(ITakeCard cardPile)
    {
        return cardPile is Deck;
    }

    public void RegisterNewCardMove(Card card)
    {
        RestoreCardColor();
        cardOnAction = card;
        cardOnAction.RegisterAction(FinishMove);
    }

    public bool CheckIfPlayerHasEnoughScoreToWinGame()
    {
        return gameManager.gameType == GameType.Quick || GetScore() >= GameManager.instance.bid.pointsToWin;
    }

    public int GetPlayerExpPointForThisGame()
    {
        return GetScore() * Constants.EXP_MULTIPLIER;
    }

    protected Card GetRandomCardFromHand()
    {
        List<Card> cards = myHand.GetCardsFromZone();
        if (cards.Count == 0)
            return null;
        Card card;
        do
        {
            card = cards.GetRandomElementFromList();
        }
        while (!IsValidCardToDiscard(card));
        return card;
    }

    protected Card GetRandomNotSequencedCard()
    {
        List<Card> cards = myHand.GetNotSequencedCards();
        Debug.Log($"[GetRandomNotSequencedCard] Found {cards.Count} not sequenced cards.");

        if (cards.Count > 0 && CheckIfItIsNotLastNotSequencedButDrawFromDiscardPileCard(cards))
        {
            Card card;
            do
            {
                card = cards.GetRandomElementFromList();
            }
            while (card != null && !IsValidCardToDiscard(card));

            Debug.Log($"[GetRandomNotSequencedCard] Selected card: {(card != null ? card.cardCode : "NULL")}");
            return card;
        }

        Card fallbackCard = GetRandomCardFromHand();
        Debug.Log($"[GetRandomNotSequencedCard] Falling back to GetRandomCardFromHand(): {(fallbackCard != null ? fallbackCard.cardCode : "NULL")}");
        return fallbackCard;
    }

    protected bool CheckIfItIsNotLastNotSequencedButDrawFromDiscardPileCard(List<Card> cards)
    {
        if (cards.Count == 1)
        {
            return IsValidCardToDiscard(cards[0]);
        }
        return true;
    }

    public Card GetBiggestCardFromHand()
    {
        List<Card> cards = myHand.GetNotSequencedCards();
        return cards.FirstOrDefault();
    }

    public bool IsValidCardToDiscard(Card card)
    {
        bool discardCardIsDifferentThanCardTaken = card != cardOnAction;
        return discardCardIsDifferentThanCardTaken || lastCardIsTakenFromDeck;
    }

    public void DiscardBiggestCard()
    {
        cardOnAction = GetBiggestCardFromHand();
        DiscardSelectedCard();
    }

    public void RegisterZoomedCard(Card card)
    {
        zoomedCard = card;
    }
   
    public int GetScore()
    {
        return playerUI.GetScore();
    }

    public void AddScore(int score)
    {
        playerUI.AddScore(score);
    }

    public Sprite GetPlayerAvatar()
    {
        return playerUI.playerAvatar.sprite;
    }

    public void SetPlayerAvatar(int id)
    {
        playerUI.RefreshAvatar(id);
    }

    public void StartTimer()
    {
        playerUI.StartTimer();
    }


    public void StopTimer()
    {
        playerUI.StopTimer();
    }

    public void UnlockHand()
    {
        myHand.UnlockZone();
    }

    public bool TimeIsOver()
    {
        return playerUI.TimeIsOver();
    }

    public void WinGame()
    {
        if (gameManager.IsThisGamePlayer(this))
        {
            PlayerManager.instance.WinGame();
        }
        else if (gameManager.IsGameWithBot())
        {
            PlayerManager.instance.WinGame();
            //PlayerManager.instance.SendFinishGameEvent(gameManager.gameData.opponentID);
        }
    }

    public void LoseGame()
    {

    }

    public virtual void ResetState()
    {
        inAction = false;
        cardOnAction = null;
        zoomedCard = null;
        lastCardIsTakenFromDeck = false;
        gameFinished = false;
        
        // 🔹 Reset new drop/game state properties for new round
        hasDropped = false;
        hasPickedCardThisTurn = false;
        // Note: Don't reset isEliminated, cumulativeScore, or dealsWon as these persist across rounds
    }
}