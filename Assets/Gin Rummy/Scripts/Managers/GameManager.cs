using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecPlayerPrefs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private GamePhase _gamePhase = GamePhase.GameEnded;


    public GameMode gameMode;


    public UnityEvent gameStateUpdated;

    public GamePhase gamePhase
    {
        get { return _gamePhase; }
        private set
        {
            passBtn.SetActive(false);
            _gamePhase = value;
            gameStateUpdated.Invoke();
            Debug.LogError("Updated GamePhase : " + value.ToString());
            SwitchStacksState();
            
            // 🔹 Update enhanced UI on phase change
            UpdateEnhancedUIOnPhaseChange();
        }
    }

    public string MatchID;
    public MatchData MatchData;
    public string ContestID;
    public TableData tableData;

    public PlayerCardsReponce playerCardsReponce;

    public Player currentPlayer { get; private set; }
    public static Action OnGameFinishedCB;

    public static GameManager instance;
    private PlayerManager playerManager;

    [Header("Cards hands")]
    public Hand thisPlayerHand;
    public Hand otherPlayerHand;

    public List<Player> playerList = new List<Player>();
    private Deck deck;
    private DiscardPile discardPile;
    private FinishPile finishPile;

    private int indexOfCurrentPlayer;
    [SerializeField] Queue<GamePhase> turnPhases = new Queue<GamePhase>();
    [SerializeField] private GameObject passBtn;
    private int playersPassed;
    public Bid bid { get; private set; }
    public GameData gameData { get; private set; }

    private Player roundWinner;
    private List<IResettable> resettables = new List<IResettable>();
    public WinType winType { get; private set; }
    public GameType gameType { get; private set; }
    public Action OnPlayerPassedCB;

    private bool isNotRedrawYet;
    private Timer swapCardsTimer;
    private float opponentFinishSwapTime;
    private float playerFinishSwapTime;
    private List<int> deadwoodValues = new List<int>();
    
    // 🔹 NEW ENHANCED GAME MANAGEMENT PROPERTIES
    [Header("Enhanced Game Management")]
    public DropButton dropButton;
    public GameModeIndicator gameModeIndicator;
    private DealsRummyManager dealsRummyManager;
    
    // Pool Rummy Management
    private List<Player> eliminatedPlayers = new List<Player>();
    private bool poolGameEnded = false;
    
    // Deals Rummy Management
    private int totalDeals = 2;
    private int currentDealNumber = 1;

    public void RegisterResettable(IResettable resettable)
    {
        resettables.AddUniqueValue(resettable);
    }

    private void Awake()
    {
        Input.multiTouchEnabled = false;
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        playerManager = FindObjectOfType<PlayerManager>();
        deck = FindObjectOfType<Deck>();
        discardPile = FindObjectOfType<DiscardPile>();
        finishPile = FindObjectOfType<FinishPile>();
        deck.OnCardDealtCB += CardDealingFinished;
        passBtn = GameObject.Find("PassBtn");
        passBtn.SetActive(false);

    }

    public void StartNewGame(GameType gameType, Bid bid, GameData gameData, PlayerCardsReponce cardsData)
    {
        if (!IsGameEnded())
            CloseThisGame();

        gamePhase = GamePhase.CardsDealing;
        playerList.Clear();
        deadwoodValues.Clear();

        MatchEndScreen.instance.TurnOffScreen();
        this.gameType = gameType;
        this.bid = bid;
        this.gameData = gameData;
        InitPlayers();
        
        // 🔹 Initialize enhanced functionality
        InitializeEnhancedUI();
        
        // Initialize game mode specific features
        if (gameMode == GameMode.Deals)
        {
            InitializeDealsRummy(2); // Default 2 deals, can be configured
        }



        //RummySocketServer.Instance.OnDealCard -= onMatchStart;
        RummySocketServer.Instance.OnStartTurn.AddListener(StartTurn);
        RummySocketServer.Instance.OnNextTurn.AddListener(NextTurn);
        RummySocketServer.Instance.OnCardFetched.AddListener(CardFetched);
        RummySocketServer.Instance.OnFinalWinner.AddListener(OnFinalWinner);
        RummySocketServer.Instance.OnPlayerDisconnected.AddListener(OnPlayerDisconnected);
        RummySocketServer.Instance.OnNewDeal.AddListener(OnNewDeal);
        RummySocketServer.Instance.OnNewRound.AddListener(OnNewRound);
        RummySocketServer.Instance.OnCardDropped.AddListener(CardDropped);
        RummySocketServer.Instance.OnTurnPassed.AddListener(turn_passed);
        RummySocketServer.Instance.OnCardTakenFromDiscard.AddListener(OnCardTakenFromPile);
        RummySocketServer.Instance.OnShufflePile.AddListener(OnShufflePile);
        RummySocketServer.Instance.OnGameFinished.AddListener(GameFinished);
        RummySocketServer.Instance.OnGameEnded.AddListener(GameEndged);
        RummySocketServer.Instance.OnResetGame.AddListener(ResetGame);
        
        // 🔹 ENHANCED GAME EVENT LISTENERS
        RummySocketServer.Instance.OnPlayerDropped.AddListener(OnPlayerDroppedFromServer);
        RummySocketServer.Instance.OnPlayerEliminated.AddListener(OnPlayerEliminatedFromServer);
        RummySocketServer.Instance.OnDealCompleted.AddListener(OnDealCompletedFromServer);
        RummySocketServer.Instance.OnDealStarted.AddListener(OnDealStartedFromServer);
        RummySocketServer.Instance.OnPoolGameEnded.AddListener(OnPoolGameEndedFromServer);
        RummySocketServer.Instance.OnCumulativeScoreUpdated.AddListener(OnCumulativeScoreUpdatedFromServer);
        RummySocketServer.Instance.OnActivePlayersUpdated.AddListener(OnActivePlayersUpdatedFromServer);



        playerCardsReponce = cardsData;
        StartNewMatch();
    }
    //private void onMatchStart(PlayerCardsReponce data)
    //{
    //    Debug.LogWarning("On Match Start Called");


    //    startMatch(data);
    //}
    //IEnumerator StartGame(PlayerCardsReponce data)
    //{
    //    yield return new WaitForEndOfFrame();

    //    // Bid newBid = new Bid(1001, GameType.Quick, CurrencyType.Chips, 0, 1, 1, 10);
    //    StartNewGame(GameType.Quick, new Bid(), new GameData(), data);

    //}
    //private void startMatch(PlayerCardsReponce data)
    //{
    //    StartCoroutine(StartGame(data));

    //}
    private void GameEndged()
    {

        FinishGame();
    }

    private void OnShufflePile(PileShuffled data)
    {


    }

    private void GameFinished(Dictionary<string, string> arg0)
    {
        AlertSlider.Instance.Show("Other Player Disconnected.\nEnding the Game.", "OK").OnPrimaryAction(() => SceneManager.LoadScene((int)Scenes.MainMenu));
    }

    public void StartNewMatch()
    {
        if (IsGameEnded())
        {
            Debug.LogError("Game already ended");
            return;
        }


        isNotRedrawYet = true;
        InitPhasesQueue();
        gameData.IncreaseSeed();
        deck.ResetState(gameData.gameRandomSeed);
        ResetResettables();
        ResetPlayers();
        Hand[] dealOrder = CreateHandDealOrder();
        deck.DealCards(dealOrder);
    }
    public async void ResetGame()
    {
        // Set the game phase to indicate that the game has ended
        _gamePhase = GamePhase.GameEnded;

        // Reset the deck state and card pool
        deck.ResetStateCardsPool();

        // Reset player states before clearing the player list
        ResetPlayers();
        playerList.Clear();

        // Prepare the deck's object pool and turn the deck back on
        deck.PrepareObjectsPool();
        deck.TurnOnDeck();

        //// Reset deck safely
        // if (gameData != null)
        //     deck.ResetState(gameData.gameRandomSeed);  // If game data exists, reset the deck using a specific seed
        // else
        //     Debug.LogError("gameData is null, skipping deck reset!");  // Log an error if gameData is null

        // Reset the player's hand and the opponent's hand, if they exist
        thisPlayerHand?.ResetState();
        otherPlayerHand?.ResetState();

        // Clear the current player reference and reset player index
        currentPlayer = null;
        indexOfCurrentPlayer = 0;

        // Clear and reinitialize the turn phases queue
        turnPhases.Clear();
        InitPhasesQueue();

        // Reset the player pass count
        playersPassed = 0;

        // Clear bid and game data
        bid = null;
        gameData = null;

        // Reset the win type and round winner
        winType = WinType.None;
        roundWinner = null;

        // Reset auxiliary game variables
        isNotRedrawYet = true;  // Mark that a redraw has not yet occurred
        playerFinishSwapTime = 0;  // Reset the player's finish swap time
        opponentFinishSwapTime = 0;  // Reset the opponent's finish swap time
        deadwoodValues.Clear();  // Clear the deadwood values

        // Safely reset UI elements if they exist
        if (FullscreenTextMessage.instance != null)
            FullscreenTextMessage.instance.CloseWindow();
        if (MatchEndScreen.instance != null)
            MatchEndScreen.instance.TurnOffScreen();
        if (RoundEndScreen.instance != null)
            RoundEndScreen.instance.TurnOffScreen();

        // Stop all active coroutines running in this script
        StopAllCoroutines();

        // Stop and destroy the swap cards timer if it exists
        if (swapCardsTimer != null)
        {
            swapCardsTimer.Kill();
            swapCardsTimer = null;
        }

    
        // Log that the game has been reset
        Debug.Log("Game has been reset.");

        // Clear the finish pile except for the first card
        ClearFinishPileExceptFirst();

        // 🔹 FIXED: Send player_ready event with proper player data (backend expects this data)
        await SendPlayerReadyEventWithData();
    }

    public void ClearFinishPileExceptFirst()
    {
        Transform finishPile = GameObject.Find("Finish Pile").transform;

        if (finishPile.childCount > 1)
        {
            for (int i = finishPile.childCount - 1; i > 0; i--) // Start from the last child
            {
                Destroy(finishPile.GetChild(i).gameObject);
            }
        }
    }
    private void OnDestroy()
    {

        RummySocketServer.Instance.Disconnect();
        RummySocketServer.Instance.OnStartTurn.RemoveListener(StartTurn);
        RummySocketServer.Instance.OnNextTurn.RemoveListener(NextTurn);
        RummySocketServer.Instance.OnCardFetched.RemoveListener(CardFetched);
        RummySocketServer.Instance.OnFinalWinner.RemoveListener(OnFinalWinner);
        RummySocketServer.Instance.OnPlayerDisconnected.RemoveListener(OnPlayerDisconnected);
        RummySocketServer.Instance.OnCardDropped.RemoveListener(CardDropped);
        RummySocketServer.Instance.OnNewDeal.RemoveListener(OnNewDeal);
        RummySocketServer.Instance.OnNewRound.RemoveListener(OnNewRound);
        RummySocketServer.Instance.OnTurnPassed.RemoveListener(turn_passed);
        RummySocketServer.Instance.OnCardTakenFromDiscard.RemoveListener(OnCardTakenFromPile);
        RummySocketServer.Instance.OnShufflePile.RemoveListener(OnShufflePile);
        RummySocketServer.Instance.OnGameFinished.RemoveListener(GameFinished);
        RummySocketServer.Instance.OnGameEnded.RemoveListener(GameEndged);
        RummySocketServer.Instance.OnResetGame.RemoveListener(ResetGame);
    }
    private void ResetResettables(bool ignoreCards = false)
    {
        for (int i = resettables.Count - 1; i >= 0; i--) // Reverse loop to avoid modifying list while iterating
        {
            if (resettables[i] == null) // Check if the object has been destroyed
            {
                Debug.LogWarning($"resettables[{i}] is null. Removing from list.");
                resettables.RemoveAt(i); // Remove destroyed objects to prevent future errors
                continue;
            }

            if (ignoreCards && resettables[i] is Card)
                continue;

            resettables[i].ResetState();
        }
    }


    private void ResetPlayers()
    {
        playersPassed = 0;
        indexOfCurrentPlayer = 0;
        for (int i = 0; i < playerList.Count; i++)
        {
            playerList[i].ResetState();
        }
    }

    private void InitPlayers()
    {
        Player p = new Player(deck);
        p.playerId = UserDataContext.Instance.UserData._id;
        p.GetPlayerData();
        p.OnFinishMoveCB += StartNextPhase;
        thisPlayerHand.playerOfThisHand = p;
        playerList.Add(p);

        Player p2 = new Player(deck, "EnemyUI");
        p2.playerId = MatchData.players.Find(p => p != UserDataContext.Instance.UserData._id);
        p2.GetPlayerData();
        // p2.OnFinishMoveCB += StartNextPhase2Player;
        otherPlayerHand.playerOfThisHand = p2;
        playerList.Add(p2);

    }

    public void StartTurn(string playerId)
    {
        Debug.LogWarning("Player Turn Start : " + playerId);

        if (playerId == UserDataContext.Instance.UserData._id)
        {
            if (deck.cachedCard == null)
            {
                FetchCard();
            }
        }
        indexOfCurrentPlayer = playerList.IndexOf(playerList.Find(p => p.playerId == playerId));
        TakeOrPassGamePhase();
    }
    public void NextTurn(string playerId)
    {
        Debug.LogWarning("Next Turn Start : " + playerId);

        if (playerId == UserDataContext.Instance.UserData._id)
        {
            if (deck.cachedCard == null)
            {
                FetchCard();
            }
            //FetchCard();
        }
        indexOfCurrentPlayer = playerList.IndexOf(playerList.Find(p => p.playerId == playerId));
        // ShowTurnDesc();
        AssignNewPlayer();
        RecreatePhasesQueue();
        StartNextPhase();
        
        // 🔹 Update enhanced UI on turn change
        UpdateEnhancedUIOnTurnChange();
        // TakeOrPassGamePhase();
    }
    public void CardFetched(FetchedCardData cardData)
    {
        deck.cachedCard = cardData.card;
        deck.SetCardOnTop(cardData);
    }
    // [Socket] new_round Received: error - [{"poolScores":{"67ab2b01dcef4cbe25d56bb4":62,"67441171daddac40ac76abab":19}}
    [System.Serializable]
    public class NewDeal
    {
        public int dealNumber;
        public Dictionary<string, int> matchWins;
    }
    public void OnNewDeal(string json)
    {
        //[Socket] new_deal Received: error - [{"dealNumber":1,
        //"cumulativeScores":{"67ab2b01dcef4cbe25d56bb4":null,"67441171daddac40ac76abab":"015"},
        //"matchWins":{"67ab2b01dcef4cbe25d56bb4":0,"67441171daddac40ac76abab":1}}]
        Debug.Log(json);
        List<NewDeal> NewDeal = JsonConvert.DeserializeObject<List<NewDeal>>(json);
        string winner = string.Empty;
        foreach (var match in NewDeal)
        {
            Debug.Log($"Winner: {match.matchWins}");
            foreach (var entry in match.matchWins)
            {
                Debug.Log($"Player {entry.Key} Wins: {entry.Value}");
                foreach (var data in playerList)
                {
                    if (data.userData._id.ToString() == entry.Key.ToString())
                    {
                        winner = data.userData.username;
                    }
                }
                if (entry.Value == 1)
                {
                    FullscreenTextMessage.instance.ShowText("First Round Winner " + winner, 5f);
                    ShowEndRoundScreenEndRoundScreen();
                }
                if (entry.Value == 2)
                {
                    FullscreenTextMessage.instance.ShowText("Second Round Winner " + winner, 5f);
                    ShowEndRoundScreenEndRoundScreen();
                    StartCoroutine(GameFinsh());
                }
            }

        }
    }
    [System.Serializable]
    public class PoolScores
    {
        public Dictionary<string, int> poolScores { get; set; }
    }
    //[Socket] new_round Received: error - [{"poolScores":{"67441171daddac40ac76abab":61,"67ab2b01dcef4cbe25d56bb4":59}}]
    public void OnNewRound(string json)
    {
        //[Socket] new_deal Received: error - [{"dealNumber":1,
        //"cumulativeScores":{"67ab2b01dcef4cbe25d56bb4":null,"67441171daddac40ac76abab":"015"},
        //"matchWins":{"67ab2b01dcef4cbe25d56bb4":0,"67441171daddac40ac76abab":1}}]
        Debug.Log(json);

        List<PoolScores> NewDeal = JsonConvert.DeserializeObject<List<PoolScores>>(json); // Deserialize JSON as a list

        string winner = string.Empty;

        foreach (var match in NewDeal) // Iterate over the list
        {
            foreach (var entry in match.poolScores) // Iterate through poolScores dictionary
            {
                Debug.Log($"Player {entry.Key} Wins: {entry.Value}");

                foreach (var data in playerList)
                {
                    if (data.userData._id.ToString() == entry.Key.ToString())
                    {
                        winner = data.userData.username;
                        break; // Exit loop early once we find the winner
                    }
                }

                if (!string.IsNullOrEmpty(winner))
                {
                    FullscreenTextMessage.instance.ShowText(" Round Winner " + winner, 3f);
                    ShowEndRoundScreenEndRoundScreen();
                }
            }
        }
     
        //foreach (var match in NewDeal) // Assuming NewDeal is a list of dictionaries
        //{
        //    foreach (var entry in match.poolScores) // Iterate through poolScores dictionary
        //    {
        //        //77261901067440
        //        Debug.Log($"Player {match.Key} Wins: {match.Value}");
        //        foreach (var data in playerList)
        //        {
        //            if (data.userData._id.ToString() == entry.Key.ToString())
        //            {
        //                winner = data.userData.username;
        //            }
        //        }
        //        //if (entry.Value == 1)
        //        //{
        //        FullscreenTextMessage.instance.ShowText(" Round Winner " + winner, 3f);
        //        ShowEndRoundScreenEndRoundScreen();
        //        //}
        //        //if (entry.Value == 2)
        //        //{
        //        //    FullscreenTextMessage.instance.ShowText("Second Round Winner " + winner, 5f);
        //        //    ShowEndRoundScreenEndRoundScreen();
        //        //    StartCoroutine(GameFinsh());
        //        //}
            

        //}
    }
    public void OnFinalWinner(string json)
    {
        Debug.Log(json);
        List<MatchWinnerData> matchWinners = JsonConvert.DeserializeObject<List<MatchWinnerData>>(json);
        string winner = string.Empty;
        foreach (var match in matchWinners)
        {
            Debug.Log($"Winner: {match.winner}");
            foreach (var entry in match.matchWins)
            {
                Debug.Log($"Player {entry.Key} Wins: {entry.Value}");
            }

            foreach (var data in playerList)
            {
                if (data.userData._id == match.winner)
                {

                    winner = data.userData.username;
                }
            }

        }
        //gamePhase = GamePhase.GameEnded;
        //if (gamePhase == GamePhase.GameEnded)
        //    EndThisGame();
        StartCoroutine(GameFinsh());
        FullscreenTextMessage.instance.ShowText(winner + " Wins ₹" + tableData.wonCoin, 5f);
    }
    public void OnPlayerDisconnected(string json)
    {
        string player = json;
        AlertSlider.Instance.Show("Other Player Disconnected.\nEnding the Game.", "OK").OnPrimaryAction(() => SceneManager.LoadScene((int)Scenes.MainMenu));

    }
    public IEnumerator GameFinsh()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene((int)Scenes.MainMenu);
    }
    [System.Serializable]
    public class MatchWinnerData
    {
        public string winner;
        public Dictionary<string, int> matchWins;
    }
    public void CardDropped(CardDropped cardData)
    {
        Debug.LogWarning("Card Dropped Event Received");

        Player player = playerList.Find(p => p.playerId == cardData.playerId);
        if (player == null)
        {
            Debug.LogError("Player not found for card drop event with ID: " + cardData.playerId);
            return;
        }

        if (currentPlayer.playerId != UserDataContext.Instance.UserData._id)
        {
            player.Discard(cardData.cardCode);
        }
        //   my player "67ab2b01dcef4cbe25d56bb4"

        //    "DUMMY_USER_1234  
    }

    public void OnCardTakenFromPile(CardDropped cardData)
    {

        if (currentPlayer.playerId != UserDataContext.Instance.UserData._id)
        {
            currentPlayer.TakeCard(discardPile);
            Debug.LogWarning($"CardTaken from pile {cardData}");
        }
    }
    public async void CardTakenFromDiscardPile(string cardCode)
    {
        await RummySocketServer.Instance.SendEvent(RummySocketEvents.card_taken_from_discard, new Dictionary<string, string>() { { "cardCode", cardCode } });

    }
    public void FinishGameFromFinishPile()
    {
        FinishGame();

        //RummyGameResult data = new RummyGameResult();
        //    data.WinnerId = RoundEndScreen.instance.winner.player.playerId;
        //    data.LoserId = RoundEndScreen.instance.loser.player.playerId;// Fixed LoserId assignment
        //    data.WinnerPoints = RoundEndScreen.instance.winner.deadwoodArea.CalculateDeadwood();
        //    data.LoserPoints = RoundEndScreen.instance.loser.deadwoodArea.CalculateDeadwood();

        //    // Creating the result object


        //    // Convert to Dictionary<string, string> before sending
        //    await RummySocketServer.Instance.SendEvent(RummySocketEvents.game_ended, data.ToDictionary());




    }



    public void turn_passed()
    {

        playersPassed++;
        if (currentPlayer != null)
            currentPlayer.StopTimer();
        Debug.LogError(" ==>> Turn Passed");
    }

    public async void FetchCard()
    {

        await RummySocketServer.Instance.SendEvent(RummySocketEvents.request_card);
    }

    private async void CardDealingFinished()
    {
        await RummySocketServer.Instance.SendEvent(RummySocketEvents.cards_received);

    }
    private async Task TurnPassed()
    {
        if (currentPlayer != null)
            currentPlayer.StopTimer();
        //if(currentPlayer.)
        await RummySocketServer.Instance.SendEvent(RummySocketEvents.pass_turn);

    }

    private Player AssignBotPlayer()
    {
        if (bid.id > Constants.BOT_MEDIUM_MAX_SCORE)
        {
            return new BotHard(deck);
        }
        return new BotMedium(deck);
    }

    private void SwapPlayers()
    {
        Player temp = playerList[0];
        playerList[0] = playerList[1];
        playerList[1] = temp;
    }

    public int GetWinPrize()
    {
        if (bid == null)
            return 0;

        return bid.win;
    }

    public CurrencyType GetGameCurrencyType()
    {
        if (bid == null)
            return CurrencyType.Chips;

        return bid.currencyType;
    }

    public int GetBidID()
    {
        if (bid == null)
            return -1;
        return bid.id;
    }



    private Hand[] CreateHandDealOrder()
    {
        Hand[] handOrder = new Hand[playerList.Count];
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].playerId == UserDataContext.Instance.UserData._id)
                handOrder[i] = thisPlayerHand;
            else
                handOrder[i] = otherPlayerHand;
        }
        return handOrder;
    }

    private void NextTurn()
    {
        Debug.Log("Next Turn Started");
        if (IsGameEnded())
        {
            Debug.LogError("Game already ended");
            return;
        }

        IncreasePlayerIndex();
        AssignNewPlayer();
        ShowTurnDesc();
        RecreatePhasesQueue();
        StartNextPhase();
    }

    private void IncreasePlayerIndex()
    {
        if (playerList.Count > 0)
            indexOfCurrentPlayer = (indexOfCurrentPlayer + 1) % playerList.Count;
    }

    private void AssignNewPlayer()
    {
        if (currentPlayer != null)
            currentPlayer.StopTimer();

        currentPlayer = playerList[indexOfCurrentPlayer];
        currentPlayer.StartTimer();
    }

    private void SwitchStacksState()
    {
        if (currentPlayer == thisPlayerHand.playerOfThisHand)
        {
            deck.FadeDeckCards(gamePhase == GamePhase.PassTakeDiscard || gamePhase == GamePhase.Discard);
            discardPile.FadeDiscardCards(gamePhase == GamePhase.TakeStockPile);
        }
        else
        {
            deck.FadeDeckCards(true);
            discardPile.FadeDiscardCards(true);
        }
    }

    private void InitPhasesQueue()
    {
        Debug.LogError($"Turn Phase Count : " + turnPhases.Count);
        turnPhases.Clear();
        turnPhases.Enqueue(GamePhase.TakeStockPile);
        turnPhases.Enqueue(GamePhase.Discard);
    }

    private void RecreatePhasesQueue()
    {
        Debug.LogError($"Turn Phase Count : " + turnPhases.Count);
        turnPhases.Clear();
        turnPhases.Enqueue(GamePhase.TakeCard);
        turnPhases.Enqueue(GamePhase.Discard);
        Debug.LogError($"New Turn Phase Count: " + turnPhases.Count);
    }

    public async void StartGame()
    {
        Debug.Log($"Starting Game with mode: {gameMode}");
        
        // 🔹 INITIALIZE GAME MODE SPECIFIC LOGIC (MOVED FROM TEST TO LIVE)
        InitializeGameModeLogic();
        
        if (IsGameEnded())
        {
            Debug.LogError("Game already ended");
            return;
        }

        gamePhase = GamePhase.CardsDealing;
        StartNewMatch();

        await Task.Delay(1000);
        TakeOrPassGamePhase();

        // 🔹 FIXED: Send player_ready event with proper player data
        await SendPlayerReadyEventWithData();
    }
    
    // 🔹 NEW: Live Game Mode Initialization (moved from test code)
    private void InitializeGameModeLogic()
    {
        Debug.Log($"[GameManager] Initializing live logic for {gameMode} mode");
        
        switch (gameMode)
        {
            case GameMode.Pool:
                InitializePoolRummyLive();
                break;
            case GameMode.Deals:
                InitializeDealsRummyLive();
                break;
            case GameMode.Points:
                InitializePointsRummyLive();
                break;
            default:
                Debug.LogWarning($"Unknown game mode: {gameMode}");
                break;
        }
    }
    
    // 🔹 NEW: Live Pool Rummy Initialization
    private void InitializePoolRummyLive()
    {
        Debug.Log("[GameManager] Setting up Pool Rummy live functionality");
        
        // Reset pool game state
        poolGameEnded = false;
        eliminatedPlayers.Clear();
        
        // Set elimination threshold based on game type
        int threshold = GetEliminationThreshold();
        Debug.Log($"Pool Rummy elimination threshold: {threshold} points");
        
        // Initialize all players for Pool Rummy
        foreach (Player player in playerList)
        {
            player.ResetForNewGame(); // Reset cumulative scores
            player.playerUI?.UpdateGameModeSpecificUI(GameMode.Pool);
            player.playerUI?.UpdateCumulativeScore(0);
            
            // Subscribe to player events for Pool Rummy
            player.OnPlayerDropped -= OnPlayerDropped;
            player.OnPlayerDropped += OnPlayerDropped;
            player.OnPlayerEliminated -= OnPlayerEliminated;
            player.OnPlayerEliminated += OnPlayerEliminated;
        }
        
        // Update UI for Pool Rummy
        gameModeIndicator?.UpdateGameModeDisplay();
        dropButton?.UpdateDropButtonVisibility();
        
        Debug.Log($"Pool Rummy initialized with {playerList.Count} players");
    }
    
    // 🔹 NEW: Live Deals Rummy Initialization
    private void InitializeDealsRummyLive()
    {
        Debug.Log("[GameManager] Setting up Deals Rummy live functionality");
        
        // Get number of deals from table data or use default
        int numberOfDeals = tableData?.gameType?.Contains("Deal") == true ? 
            GetDealsFromGameType(tableData.gameType) : Constants.DEFAULT_DEALS_COUNT;
        
        Debug.Log($"Deals Rummy: {numberOfDeals} deals configured");
        
        // Initialize deals management
        InitializeDealsRummy(numberOfDeals);
        
        // Initialize all players for Deals Rummy
        foreach (Player player in playerList)
        {
            player.ResetForNewGame(); // Reset for first deal
            player.playerUI?.UpdateGameModeSpecificUI(GameMode.Deals);
            player.playerUI?.UpdateCumulativeScore(0);
        }
        
        // Update UI for Deals Rummy
        gameModeIndicator?.UpdateGameModeDisplay();
        gameModeIndicator?.OnDealChanged();
        
        Debug.Log($"Deals Rummy initialized: Deal 1 of {numberOfDeals}");
    }
    
    // 🔹 NEW: Live Points Rummy Initialization (enhanced)
    private void InitializePointsRummyLive()
    {
        Debug.Log("[GameManager] Setting up Points Rummy live functionality");
        
        // Points Rummy doesn't need special initialization as it's per-round
        // Just ensure UI is updated
        foreach (Player player in playerList)
        {
            player.playerUI?.UpdateGameModeSpecificUI(GameMode.Points);
        }
        
        gameModeIndicator?.UpdateGameModeDisplay();
        
        // Calculate and display per-point value
        double perPointValue = GetPerPointValue();
        Debug.Log($"Points Rummy per-point value: ₹{perPointValue:F2}");
    }
    
    // 🔹 NEW: Extract deals count from game type string
    private int GetDealsFromGameType(string gameType)
    {
        if (string.IsNullOrEmpty(gameType)) return Constants.DEFAULT_DEALS_COUNT;
        
        // Extract number from strings like "2Deal", "3Deal", etc.
        string numberPart = gameType.Replace("Deal", "").Replace("Deals", "");
        if (int.TryParse(numberPart, out int deals))
        {
            return Mathf.Clamp(deals, 1, Constants.MAX_DEALS_COUNT);
        }
        
        return Constants.DEFAULT_DEALS_COUNT;
    }

    public void CollectCards(bool isCurrentPlayer)
    {
        if (isCurrentPlayer)
        {
            thisPlayerHand.ReturnCardsToDeck();
            playerFinishSwapTime = Time.time + Constants.DEAL_ANIM_TIME * Constants.CARDS_IN_START_HAND;
        }
        else
        {
            otherPlayerHand.ReturnCardsToDeck();
            opponentFinishSwapTime = Time.time + Constants.DEAL_ANIM_TIME * Constants.CARDS_IN_START_HAND;
        }
    }

    private void PerformCardsRedraw(bool playerAgreed, bool opponentAgreed)
    {
        isNotRedrawYet = false;
        if (!playerAgreed && !opponentAgreed)
        {
            TakeOrPassGamePhase();
        }
        else
        {
            Hand[] dealOrder = new Hand[] { };
            if (playerAgreed)
            {
                dealOrder = new Hand[] { thisPlayerHand };
            }
            else
            {
                playerFinishSwapTime = 0;
            }

            if (opponentAgreed)
            {
                dealOrder = new Hand[] { otherPlayerHand };
            }
            else
            {
                opponentFinishSwapTime = 0;
            }

            if (playerAgreed && opponentAgreed)
                dealOrder = CreateHandDealOrder();

            discardPile.ReturnCardsToDeck();
            gameData.IncreaseSeed();
            ResetResettables(true);
            ResetPlayers();

            float timer = Constants.DEAL_ANIM_TIME * 2;
            float longerTime = Mathf.Max(playerFinishSwapTime, opponentFinishSwapTime, Time.time);
            timer += (longerTime - Time.time);

            swapCardsTimer = new Timer(timer, () => deck.PrepareDeckToReorder(gameData.gameRandomSeed, dealOrder));

        }

    }

    public bool IsGameWithBot()
    {
        return otherPlayerHand.playerOfThisHand is AIPlayer;
    }

    private void TakeOrPassGamePhase()
    {
        if (IsGameEnded())
        {
            Debug.LogError("Game already ended");
            return;
        }
        AssignNewPlayer();
        // if (playersPassed == playerList.Count)
        // {
        //     Debug.LogWarning("Starting Next Phase ");
        //     StartNextPhase();
        //     return;
        // }
        // gamePhase = GamePhase.PassTakeDiscard;
        // passBtn.SetActive(IsThisGamePlayerAndTimeIsNotOver());
        passBtn.SetActive(false);

        Debug.LogWarning("Starting Next Phase ");
        StartNextPhase();


    }

    private void StartNextPhase()
    {
        Debug.Log("Starting Next Phase");
        if (gamePhase == GamePhase.RoundEnded)
        {
            EndThisGame();
        }
        else if (turnPhases.Count > 0)
        {
            // if (ShouldSkipTakeCardPhase())
            //     gamePhase = turnPhases.Dequeue();

            if (turnPhases.Count > 0)
                gamePhase = turnPhases.Dequeue();
            CheckIfNextPlayerIsBot();

        }
        else
        {
            if (!currentPlayer.gameFinished)
                TurnFinished();
            // NextTurn();
        }

    }


    private void ShowTurnDesc()
    {
        FullscreenTextMessage.instance.ShowText(currentPlayer.name + " turn.");
    }


    private void EndThisGame()
    {
        deadwoodValues.Add(thisPlayerHand.GetDeadwoodScore());
        Debug.Log(thisPlayerHand.GetDeadwoodScore());
        OnGameFinishedCB.RunAction();
        ShowEndRoundScreenEndRoundScreen();
        // if (winType == WinType.Knock)
        // {
        //     ShowEndRoundScreenEndRoundScreen();
        // }
        // else
        // {
        //     FullscreenTextMessage.instance.ShowText(winType.ToString());
        //     new Timer(3, ShowEndRoundScreenEndRoundScreen);
        // }
    }

    public void ShowEndRoundScreenEndRoundScreen()
    {
        currentPlayer.gameFinished = false;
        RoundEndScreen.instance.EndRound(new Hand[] { thisPlayerHand, otherPlayerHand });
    }

    private bool ShouldSkipTakeCardPhase()
    {
        bool isPassOrTakePhase = gamePhase == GamePhase.PassTakeDiscard;
        bool playerNotPassed = playersPassed < playerList.Count;
        return isPassOrTakePhase && playerNotPassed;
    }

    Action botAction;

    private void CheckIfNextPlayerIsBot()
    {

        if (currentPlayer.TimeIsOver())
        {
            currentPlayer.PerformAction();
        }
        else if (currentPlayer is AIPlayer)
        {
            AIPlayer bot = currentPlayer as AIPlayer;
            botAction = GetCurrentTurnProperAction(bot);

            float decisionTime = Randomizer.GetRandomNumber(Constants.BOT_ACTION_MIN_DELAY, Constants.BOT_ACTION_MAX_DELAY);
            new Timer(decisionTime, DelayedBotAction);
        }
    }

    public Action GetCurrentTurnProperAction(Player player)
    {
        Action action = null;
        switch (gamePhase)
        {
            case GamePhase.PassTakeDiscard:
                action = player.PassOrTake;
                passBtn.SetActive(false);
                break;
            case GamePhase.TakeStockPile:
                action = player.TakeStockPile;
                break;
            case GamePhase.TakeCard:
                action = player.TakeCard;
                break;
            case GamePhase.Discard:
                action = player.Discard;
                break;
            default:
                break;
        }
        Debug.LogWarning($"Get Action : {action}");
        return action;
    }

    void DelayedBotAction()
    {
        if (gamePhase != GamePhase.GameEnded)
            botAction.RunAction();
    }

    public async void OnPass()
    {
        passBtn.SetActive(false);
        if (IsThisGamePlayer())
        {
            OnPlayerPassedCB.RunAction();
        }
        currentPlayer.UnlockHand();
        if (currentPlayer.gameFinished)
            await TurnPassed();
        // IncreasePlayerIndex();

        // TurnFinished();

        // TakeOrPassGamePhase();
    }

    public async void TurnFinished()
    {
        Debug.LogWarning($"Turn Finished");
        if (currentPlayer != null)
            currentPlayer.StopTimer();
        if (currentPlayer.playerId == UserDataContext.Instance.UserData._id)
        {
            await RummySocketServer.Instance.SendEvent(RummySocketEvents.end_turn);
            Debug.LogWarning($"Sending Next Turn Event");
        }
    }


    public void FinishGame()
    {
        winType = WinType.Gin;
        roundWinner = currentPlayer;
        if (!tableData.gameMode.Contains("Deals"))
        {
            gamePhase = GamePhase.RoundEnded;
        }
        EndThisGame();  //comment by jatan0 07-03-25
    }
    public void OnKnock()
    {
        FinishRound(WinType.Knock);
        currentPlayer.DiscardBiggestCard();
    }

    public void OnGin()
    {
        FinishRound(WinType.Gin);
        currentPlayer.DiscardBiggestCard();
    }

    public void OnBigGin()
    {
        FinishRound(WinType.BigGin);
        //EndThisGame();
    }

    private void FinishRound(WinType type)
    {
        winType = type;
        roundWinner = currentPlayer;
        gamePhase = GamePhase.RoundEnded;
    }

    public Player GetWinnerOfMatch()
    {
        return roundWinner;
    }

    public void TryDiscardCard(Card card)
    {
        if (IsValidTimeToDiscard())
            currentPlayer.Discard(card);
    }

    public void TryAddThisCardAsThisPlayerZoomedCard(Card card)
    {
        if (IsThisPlayerTurn(thisPlayerHand.playerOfThisHand) && card != null)
        {
            currentPlayer.RegisterZoomedCard(card);
        }

    }

    public bool IsValidCardToDiscard(Card card)
    {
        return currentPlayer.IsValidCardToDiscard(card);
    }

    public bool IsValidTimeToDiscard()
    {
        return IsValidGamePhase(GamePhase.Discard);
    }

    public bool IsValidTimeToTakeCard()
    {

        return IsValidGamePhase(GamePhase.TakeCard);
    }

    public bool IsPassOrTakePhase()
    {
        return IsValidGamePhase(GamePhase.PassTakeDiscard);
    }

    public bool IsTakeFromStockPilePhase()
    {
        return IsValidGamePhase(GamePhase.TakeStockPile);
    }

    private bool IsValidGamePhase(GamePhase gPhase)
    {
        return gamePhase == gPhase && IsThisGamePlayerAndTimeIsNotOver();
    }



    public bool IsThisGamePlayerAndTimeIsNotOver()
    {
        if (currentPlayer == null)
            return false;

        return IsThisGamePlayer() && currentPlayer.TimeIsOver() == false;
    }

    public bool IsThisGamePlayer()
    {
        return currentPlayer == thisPlayerHand.playerOfThisHand;
    }

    public float GetLastGameAverageDeadwood()
    {
        if (deadwoodValues.Count == 0)
            return thisPlayerHand.GetDeadwoodScore();
        return (float)deadwoodValues.Average();
    }

    public bool IsThisGamePlayer(Player player)
    {
        return player == thisPlayerHand.playerOfThisHand;
    }

    public bool IsThisPlayerTurn(Player player)
    {
        return currentPlayer == player;
    }

    public bool isRoundEnded()
    {
        return gamePhase == GamePhase.RoundEnded;
    }

    public bool IsGameEnded()
    {
        return gamePhase == GamePhase.GameEnded;
    }

    public void CloseThisGame()
    {
        playerManager.SaveGameStatsData(GetLastGameAverageDeadwood(), thisPlayerHand.playerOfThisHand.GetScore());
        if (swapCardsTimer != null)
            swapCardsTimer.Kill();
        OnGameFinishedCB.RunAction();
        deck.TurnOffDeck();
        FullscreenTextMessage.instance.CloseWindow();
        gamePhase = GamePhase.GameEnded;
    }



    public void OnLeaveGame()
    {
        DecisionPopup.instance.Show("Leave the current game? This will count as a loss.", ConfirmLeaveGame, null);
    }

    private void ConfirmLeaveGame()
    {
        CloseThisGame();
        StartScreen.instance.TurnOnScreen();
    }

    public void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            ShowEndRoundScreenEndRoundScreen();
        }
    }
    public double GetPerPointValue()
    {
        return (double)(tableData.totalBet / Constants.MAX_POINTS_RUMMY); // Define your logic accordingly
    }
    public int totalPoolAfterRakeDeduction(int totalPool)
    {
        int platformFee = GetPlatformFee(totalPool);
        return totalPool - platformFee;
    }

    public int GetDealsWinningAmount()
    {
        int totalPool = playerList.Count * (int)tableData.bet;
        return totalPoolAfterRakeDeduction(totalPool);
    }

    public int GetPoolWinningAmount()
    {
        int totalPool = playerList.Count * (int)tableData.bet;
        return totalPool - GetPlatformFee(totalPool);
    }
    public int GetPlatformFee(int winningAmount)
    {
        return Mathf.FloorToInt((winningAmount * (int)tableData.rakePercentage) / 100f);
    }
    public bool CheckIfOnlyOnePlayerRemains()
    {
        int eliminationScore = (tableData.gameType == "Pool101") ? Constants.POOL_101_ELIMINATION_SCORE : Constants.POOL_201_ELIMINATION_SCORE;

        int activePlayers = 0;

        foreach (Player player in playerList)
        {
            if (player.GetScore() < eliminationScore) // Player is still active
            {
                activePlayers++;
            }
        }

        return activePlayers == 1; // Game ends when only 1 player remains
    }
    public void ShakeValidDeck()
    {
        switch (gamePhase)
        {
            case GamePhase.TakeStockPile:
                deck.Shake();
                break;
            case GamePhase.PassTakeDiscard:
                discardPile.Shake();
                break;
        }
    }
    
    // 🔹 NEW ENHANCED GAME FUNCTIONALITY
    
    #region Drop Functionality
    public void OnPlayerDropped(Player player)
    {
        if (player == null) return;
        
        Debug.Log($"GameManager: Player {player.name} has dropped from the game");
        
        // Update player UI to show dropped state
        player.playerUI?.SetDroppedState(true);
        
        // 🔹 SEND ACTIVE PLAYERS UPDATE TO SERVER
        SendActivePlayersUpdate();
        
        // In Pool Rummy, check if only one player remains
        if (gameMode == GameMode.Pool)
        {
            CheckPoolGameEnd();
        }
        
        // Continue to next player if current player dropped
        if (currentPlayer == player)
        {
            StartNextPhase();
        }
        
        // Update UI
        dropButton?.UpdateDropButtonVisibility();
        gameModeIndicator?.OnGameStateChanged();
    }
    
    private void CheckPoolGameEnd()
    {
        if (poolGameEnded) return;
        
        List<Player> activePlayers = GetActivePlayers();
        
        if (activePlayers.Count <= 1)
        {
            poolGameEnded = true;
            Player winner = activePlayers.Count == 1 ? activePlayers[0] : null;
            EndPoolGame(winner);
        }
    }
    
    private void EndPoolGame(Player winner)
    {
        Debug.Log($"Pool game ended. Winner: {winner?.name ?? "No winner"}");
        
        // 🔹 SEND POOL GAME ENDED EVENT TO SERVER
        SendPoolGameEndedEvent(winner);
        
        if (winner != null)
        {
            int winningAmount = GetPoolWinningAmount();
            FullscreenTextMessage.instance.ShowText($"{winner.name} Wins Pool!\n₹{winningAmount}", 5f);
            winner.AddScore(winningAmount);
        }
        
        gamePhase = GamePhase.GameEnded;
        ShowEndRoundScreenEndRoundScreen();
    }
    
    public List<Player> GetActivePlayers()
    {
        return playerList.Where(p => !p.hasDropped && !p.isEliminated).ToList();
    }
    #endregion
    
    #region Elimination System
    public void OnPlayerEliminated(Player player)
    {
        if (player == null || eliminatedPlayers.Contains(player)) return;
        
        eliminatedPlayers.Add(player);
        Debug.Log($"Player {player.name} has been eliminated!");
        
        // Update player UI
        player.playerUI?.SetEliminatedState(true);
        
        // Show elimination notification
        FullscreenTextMessage.instance.ShowText($"{player.name} Eliminated!", 3f);
        
        // Check if game should end
        if (gameMode == GameMode.Pool)
        {
            CheckPoolGameEnd();
        }
        
        // Update UI
        gameModeIndicator?.OnPlayerEliminated();
    }
    
    public int GetEliminationThreshold()
    {
        if (tableData?.gameType == "Pool101")
            return Constants.POOL_101_ELIMINATION_SCORE;
        else if (tableData?.gameType == "Pool201")
            return Constants.POOL_201_ELIMINATION_SCORE;
        
        return Constants.POOL_101_ELIMINATION_SCORE; // Default
    }
    
    public int GetActivePlayersCount()
    {
        return GetActivePlayers().Count;
    }
    
    // 🔹 NEW: Missing method for checking one player remains
    //public bool CheckIfOnlyOnePlayerRemains()
    //{
    //    return GetActivePlayersCount() <= 1;
    //}
    
    //// 🔹 NEW: Get Pool winning amount
    //public int GetPoolWinningAmount()
    //{
    //    double totalPool = bid?.totalBet ?? 100;
    //    int platformFee = GetPlatformFee((int)totalPool);
    //    return (int)(totalPool - platformFee);
    //}
    
    //// 🔹 NEW: Get Deals winning amount  
    //public int GetDealsWinningAmount()
    //{
    //    double totalPool = bid?.totalBet ?? 100;
    //    int platformFee = GetPlatformFee((int)totalPool);
    //    return (int)(totalPool - platformFee);
    //}
    #endregion
    
    #region Deals Rummy Management
    public void InitializeDealsRummy(int numberOfDeals)
    {
        totalDeals = numberOfDeals;
        currentDealNumber = 1;
        
        if (dealsRummyManager == null)
            dealsRummyManager = FindObjectOfType<DealsRummyManager>();
        
        dealsRummyManager?.InitializeDealsRummy(numberOfDeals);
    }
    
    public void StartNewDeal()
    {
        currentDealNumber++;
        
        // 🔹 SEND DEAL STARTED EVENT TO SERVER
        SendDealStartedEvent();
        
        // Reset game state for new deal
        ResetGame();
        
        // Start new round
        StartGame();
    }
    
    public void CompleteDeal(Player winner)
    {
        if (gameMode == GameMode.Deals && dealsRummyManager != null)
        {
            // 🔹 SEND DEAL COMPLETED EVENT TO SERVER
            SendDealCompletedEvent(winner);
            
            dealsRummyManager.CompleteDeal(winner, playerList);
        }
    }
    
    public int GetTotalDeals()
    {
        if (dealsRummyManager != null)
            return dealsRummyManager.GetTotalDeals();
        return totalDeals;
    }
    
    public int GetCurrentDealNumber()
    {
        if (dealsRummyManager != null)
            return dealsRummyManager.GetCurrentDealNumber();
        return currentDealNumber;
    }
    #endregion
    
    #region Game Mode Specific Win Conditions
    public bool CheckGameModeSpecificWinConditions()
    {
        switch (gameMode)
        {
            case GameMode.Points:
                return CheckPointsRummyWin();
            case GameMode.Pool:
                return CheckPoolRummyWin();
            case GameMode.Deals:
                return CheckDealsRummyWin();
            default:
                return false;
        }
    }
    
    private bool CheckPointsRummyWin()
    {
        // Points Rummy ends after each round
        return true;
    }
    
    private bool CheckPoolRummyWin()
    {
        // Pool Rummy continues until only one player remains
        return GetActivePlayersCount() <= 1;
    }
    
    private bool CheckDealsRummyWin()
    {
        // Deals Rummy continues until all deals are completed
        return dealsRummyManager?.IsLastDeal() ?? false;
    }
    #endregion
    
    #region Enhanced UI Integration
    private void InitializeEnhancedUI()
    {
        // Find UI components if not assigned
        if (dropButton == null)
            dropButton = FindObjectOfType<DropButton>();
        
        if (gameModeIndicator == null)
            gameModeIndicator = FindObjectOfType<GameModeIndicator>();
        
        if (dealsRummyManager == null)
            dealsRummyManager = FindObjectOfType<DealsRummyManager>();
        
        // Setup player event handlers
        foreach (Player player in playerList)
        {
            player.OnPlayerDropped += OnPlayerDropped;
            player.OnPlayerEliminated += OnPlayerEliminated;
            
            // Update player UI for current game mode
            player.playerUI?.UpdateGameModeSpecificUI(gameMode);
        }
        
        // Update UI components
        dropButton?.UpdateDropButtonVisibility();
        gameModeIndicator?.UpdateGameModeDisplay();
    }
    
    public void UpdateEnhancedUIOnPhaseChange()
    {
        dropButton?.OnGamePhaseChanged();
        gameModeIndicator?.OnGameStateChanged();
    }
    
    public void UpdateEnhancedUIOnTurnChange()
    {
        dropButton?.OnTurnChanged();
    }
    #endregion
    
    // 🔹 NEW SERVER COMMUNICATION METHODS
    
    #region Enhanced Server Communication
    public async void SendDealCompletedEvent(Player winner)
    {
        try
        {
            if (gameMode != GameMode.Deals) return;
            
            var dealData = new DealCompletedData
            {
                winnerId = winner.playerId,
                winnerName = winner.name,
                dealNumber = GetCurrentDealNumber(),
                totalDeals = GetTotalDeals(),
                playerScores = GetPlayerScoresDictionary(),
                cumulativeScores = GetCumulativeScoresDictionary(),
                dealsWon = GetDealsWonDictionary(),
                matchId = SecurePlayerPrefs.GetString(Appinop.Constants.KMatchId),
                isFinalDeal = GetCurrentDealNumber() >= GetTotalDeals()
            };
            
            await RummySocketServer.Instance.SendEnhancedEvent(RummySocketEvents.deal_completed, dealData);
            Debug.Log($"Sent deal_completed event for deal {dealData.dealNumber}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send deal_completed event: {e.Message}");
        }
    }
    
    public async void SendDealStartedEvent()
    {
        try
        {
            if (gameMode != GameMode.Deals) return;
            
            var dealStartData = new DealStartedData
            {
                dealNumber = GetCurrentDealNumber(),
                totalDeals = GetTotalDeals(),
                matchId = SecurePlayerPrefs.GetString(Appinop.Constants.KMatchId),
                activePlayers = GetActivePlayers().Select(p => p.playerId).ToList(),
                cumulativeScores = GetCumulativeScoresDictionary()
            };
            
            await RummySocketServer.Instance.SendEnhancedEvent(RummySocketEvents.deal_started, dealStartData);
            Debug.Log($"Sent deal_started event for deal {dealStartData.dealNumber}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send deal_started event: {e.Message}");
        }
    }
    
    public async void SendPoolGameEndedEvent(Player winner)
    {
        try
        {
            if (gameMode != GameMode.Pool) return;
            
            var poolEndData = new PoolGameEndedData
            {
                winnerId = winner?.playerId ?? "",
                winnerName = winner?.name ?? "No Winner",
                winningAmount = GetPoolWinningAmount(),
                poolType = tableData?.gameType ?? "Pool101",
                finalScores = GetCumulativeScoresDictionary(),
                eliminatedPlayers = eliminatedPlayers.Select(p => p.playerId).ToList(),
                matchId = SecurePlayerPrefs.GetString(Appinop.Constants.KMatchId)
            };
            
            await RummySocketServer.Instance.SendEnhancedEvent(RummySocketEvents.pool_game_ended, poolEndData);
            Debug.Log($"Sent pool_game_ended event. Winner: {winner?.name}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send pool_game_ended event: {e.Message}");
        }
    }
    
    public async void SendActivePlayersUpdate()
    {
        try
        {
            var activePlayersData = new ActivePlayersData
            {
                activePlayers = GetActivePlayers().Select(p => p.playerId).ToList(),
                droppedPlayers = playerList.Where(p => p.hasDropped).Select(p => p.playerId).ToList(),
                eliminatedPlayers = eliminatedPlayers.Select(p => p.playerId).ToList(),
                activeCount = GetActivePlayersCount(),
                gameMode = gameMode.ToString(),
                matchId = SecurePlayerPrefs.GetString(Appinop.Constants.KMatchId)
            };
            
            await RummySocketServer.Instance.SendEnhancedEvent(RummySocketEvents.active_players_updated, activePlayersData);
            Debug.Log($"Sent active_players_updated event. Active: {activePlayersData.activeCount}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send active_players_updated event: {e.Message}");
        }
    }
    
    // Helper methods for data collection
    private Dictionary<string, int> GetPlayerScoresDictionary()
    {
        var scores = new Dictionary<string, int>();
        foreach (Player player in playerList)
        {
            scores[player.playerId] = player.GetFinalScore();
        }
        return scores;
    }
    
    private Dictionary<string, int> GetCumulativeScoresDictionary()
    {
        var scores = new Dictionary<string, int>();
        foreach (Player player in playerList)
        {
            scores[player.playerId] = player.cumulativeScore;
        }
        return scores;
    }
    
    private Dictionary<string, int> GetDealsWonDictionary()
    {
        var dealsWon = new Dictionary<string, int>();
        foreach (Player player in playerList)
        {
            dealsWon[player.playerId] = player.dealsWon;
        }
        return dealsWon;
    }
    
    // 🔹 NEW: Send player_ready event with proper data (fixes backend communication issue)
    private async Task SendPlayerReadyEventWithData()
    {
        try
        {
            Debug.Log("🔍 [FRONTEND DEBUG] ===== STARTING PLAYER_READY EVENT FLOW =====");
            Debug.Log($"🔍 [FRONTEND DEBUG] Method: SendPlayerReadyEventWithData called");
            
            // Get current player information
            Player currentPlayerData = thisPlayerHand?.playerOfThisHand;
            if (currentPlayerData == null)
            {
                Debug.LogError("❌ [FRONTEND DEBUG] No current player found for player_ready event!");
                return;
            }
            
            Debug.Log($"🔍 [FRONTEND DEBUG] Current player found: {currentPlayerData.name}");
            Debug.Log($"🔍 [FRONTEND DEBUG] Current player ID: {currentPlayerData.playerId}");
            
            // Create comprehensive player ready data
            PlayerReadyData playerReadyData = new PlayerReadyData
            {
                playerId = currentPlayerData.playerId ?? UserDataContext.Instance.UserData._id,
                playerName = currentPlayerData.userData?.username ?? UserDataContext.Instance.UserData.username,
                matchId = SecurePlayerPrefs.GetString(Appinop.Constants.KMatchId),
                gameMode = gameMode.ToString(),
                gameType = tableData?.gameType ?? "Unknown",
                isReady = true,
                readyTime = DateTime.UtcNow,
                playerStatus = "ready",
                currentPlayers = playerList?.Count ?? 0,
                maxPlayers = tableData?.gameType?.Contains("2") == true ? 2 : 
                           tableData?.gameType?.Contains("4") == true ? 4 : 6,
                tableId = tableData?._id,
                walletBalance = UserDataContext.Instance.UserData.walletCoins,
                clientVersion = Application.version,
                deviceInfo = $"{SystemInfo.deviceModel}_{SystemInfo.operatingSystem}"
            };
            
            Debug.Log("🔍 [FRONTEND DEBUG] ===== PLAYER READY DATA CREATED =====");
            Debug.Log($"🔍 [FRONTEND DEBUG] playerId: {playerReadyData.playerId}");
            Debug.Log($"🔍 [FRONTEND DEBUG] playerName: {playerReadyData.playerName}");
            Debug.Log($"🔍 [FRONTEND DEBUG] matchId: {playerReadyData.matchId}");
            Debug.Log($"🔍 [FRONTEND DEBUG] gameMode: {playerReadyData.gameMode}");
            Debug.Log($"🔍 [FRONTEND DEBUG] gameType: {playerReadyData.gameType}");
            Debug.Log($"🔍 [FRONTEND DEBUG] isReady: {playerReadyData.isReady}");
            Debug.Log($"🔍 [FRONTEND DEBUG] playerStatus: {playerReadyData.playerStatus}");
            Debug.Log($"🔍 [FRONTEND DEBUG] currentPlayers: {playerReadyData.currentPlayers}");
            Debug.Log($"🔍 [FRONTEND DEBUG] maxPlayers: {playerReadyData.maxPlayers}");
            Debug.Log($"🔍 [FRONTEND DEBUG] walletBalance: {playerReadyData.walletBalance}");
            
            Debug.Log("🔍 [FRONTEND DEBUG] ===== SENDING TO SOCKET SERVER =====");
            Debug.Log($"🔍 [FRONTEND DEBUG] About to call RummySocketServer.Instance.SendEnhancedEvent");
            Debug.Log($"🔍 [FRONTEND DEBUG] Event: RummySocketEvents.player_ready");
            Debug.Log($"🔍 [FRONTEND DEBUG] Socket connected: {RummySocketServer.Instance != null}");
            
            // Send enhanced player_ready event with comprehensive data
            await RummySocketServer.Instance.SendEnhancedEvent(RummySocketEvents.player_ready, playerReadyData);
            
            Debug.Log("✅ [FRONTEND DEBUG] SendEnhancedEvent call completed successfully!");
            Debug.Log("🔍 [FRONTEND DEBUG] ===== WAITING FOR BACKEND RESPONSE =====");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ [FRONTEND DEBUG] Failed to send player_ready event: {e.Message}");
            Debug.LogError($"❌ [FRONTEND DEBUG] Stack trace: {e.StackTrace}");
            Debug.LogError($"❌ [FRONTEND DEBUG] Inner exception: {e.InnerException?.Message}");
            
            // Try fallback with basic data
            Debug.Log("🔍 [FRONTEND DEBUG] ===== ATTEMPTING FALLBACK =====");
            await SendBasicPlayerReadyFallback();
        }
    }
    
    // 🔹 NEW: Fallback method for player_ready if enhanced method fails
    private async Task SendBasicPlayerReadyFallback()
    {
        try
        {
            Debug.Log("[GameManager] Attempting fallback player_ready with basic data...");
            
            var basicData = new Dictionary<string, string>
            {
                { "playerId", UserDataContext.Instance.UserData._id },
                { "matchId", SecurePlayerPrefs.GetString(Appinop.Constants.KMatchId) },
                { "status", "ready" },
                { "gameMode", gameMode.ToString() }
            };
            
            await RummySocketServer.Instance.SendEvent(RummySocketEvents.player_ready, basicData);
            Debug.Log("[GameManager] ✅ Fallback player_ready event sent!");
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameManager] ❌ Even fallback player_ready failed: {e.Message}");
        }
    }
    #endregion
    
    // 🔹 SERVER EVENT HANDLERS
    
    #region Server Event Handlers
    public void OnPlayerDroppedFromServer(PlayerDroppedData data)
    {
        Debug.Log($"Received player_dropped from server: {data.playerName}");
        
        Player player = playerList.Find(p => p.playerId == data.playerId);
        if (player != null && !player.hasDropped)
        {
            // Sync local state with server
            player.SetDroppedState(true);
            player.SetCumulativeScore(data.cumulativeScore);
            
            // Update UI
            player.playerUI?.SetDroppedState(true);
            player.playerUI?.UpdateCumulativeScore(data.cumulativeScore);
            
            gameModeIndicator?.OnGameStateChanged();
        }
    }
    
    public void OnPlayerEliminatedFromServer(PlayerEliminatedData data)
    {
        Debug.Log($"Received player_eliminated from server: {data.playerName}");
        
        Player player = playerList.Find(p => p.playerId == data.playerId);
        if (player != null && !player.isEliminated)
        {
            // Sync local state with server
            player.SetEliminatedState(true);
            player.SetCumulativeScore(data.eliminationScore);
            
            // Update UI
            player.playerUI?.SetEliminatedState(true);
            player.playerUI?.UpdateCumulativeScore(data.eliminationScore);
            
            // Show notification
            FullscreenTextMessage.instance.ShowText($"{data.playerName} Eliminated!", Constants.ELIMINATION_NOTIFICATION_TIME);
            
            gameModeIndicator?.OnPlayerEliminated();
        }
    }
    
    public void OnDealCompletedFromServer(DealCompletedData data)
    {
        Debug.Log($"Received deal_completed from server: Deal {data.dealNumber} won by {data.winnerName}");
        
        // Sync deal state
        currentDealNumber = data.dealNumber;
        
        // Update player scores and deals won
        foreach (var scoreEntry in data.cumulativeScores)
        {
            Player player = playerList.Find(p => p.playerId == scoreEntry.Key);
            if (player != null)
            {
                player.SetCumulativeScore(scoreEntry.Value);
                player.playerUI?.UpdateCumulativeScore(scoreEntry.Value);
            }
        }
        
        foreach (var dealEntry in data.dealsWon)
        {
            Player player = playerList.Find(p => p.playerId == dealEntry.Key);
            if (player != null)
            {
                player.SetDealsWon(dealEntry.Value);
            }
        }
        
        gameModeIndicator?.OnDealChanged();
    }
    
    public void OnDealStartedFromServer(DealStartedData data)
    {
        Debug.Log($"Received deal_started from server: Starting deal {data.dealNumber}");
        
        // Sync deal state
        currentDealNumber = data.dealNumber;
        totalDeals = data.totalDeals;
        
        // Update cumulative scores
        foreach (var scoreEntry in data.cumulativeScores)
        {
            Player player = playerList.Find(p => p.playerId == scoreEntry.Key);
            if (player != null)
            {
                player.SetCumulativeScore(scoreEntry.Value);
                player.playerUI?.UpdateCumulativeScore(scoreEntry.Value);
            }
        }
        
        gameModeIndicator?.OnDealChanged();
    }
    
    public void OnPoolGameEndedFromServer(PoolGameEndedData data)
    {
        Debug.Log($"Received pool_game_ended from server: Winner {data.winnerName}");
        
        // Sync final scores
        foreach (var scoreEntry in data.finalScores)
        {
            Player player = playerList.Find(p => p.playerId == scoreEntry.Key);
            if (player != null)
            {
                player.SetCumulativeScore(scoreEntry.Value);
                player.playerUI?.UpdateCumulativeScore(scoreEntry.Value);
            }
        }
        
        // Find winner and update
        Player winner = playerList.Find(p => p.playerId == data.winnerId);
        if (winner != null)
        {
            winner.AddScore(data.winningAmount);
            FullscreenTextMessage.instance.ShowText($"{data.winnerName} Wins Pool!\n₹{data.winningAmount}", 5f);
        }
        
        // End game
        gamePhase = GamePhase.GameEnded;
        ShowEndRoundScreenEndRoundScreen();
    }
    
    public void OnCumulativeScoreUpdatedFromServer(CumulativeScoreData data)
    {
        Debug.Log($"Received cumulative_score_updated from server: {data.playerId} scored {data.scoreThisRound}");
        
        Player player = playerList.Find(p => p.playerId == data.playerId);
        if (player != null)
        {
            player.SetCumulativeScore(data.cumulativeScore);
            player.SetEliminatedState(data.isEliminated);
            
            player.playerUI?.UpdateCumulativeScore(data.cumulativeScore);
            if (data.isEliminated)
            {
                player.playerUI?.SetEliminatedState(true);
            }
        }
    }
    
    public void OnActivePlayersUpdatedFromServer(ActivePlayersData data)
    {
        Debug.Log($"Received active_players_updated from server: {data.activeCount} active players");
        
        // Sync dropped and eliminated states
        foreach (string droppedPlayerId in data.droppedPlayers)
        {
            Player player = playerList.Find(p => p.playerId == droppedPlayerId);
            if (player != null && !player.hasDropped)
            {
                player.SetDroppedState(true);
                player.playerUI?.SetDroppedState(true);
            }
        }
        
        foreach (string eliminatedPlayerId in data.eliminatedPlayers)
        {
            Player player = playerList.Find(p => p.playerId == eliminatedPlayerId);
            if (player != null && !player.isEliminated)
            {
                player.SetEliminatedState(true);
                player.playerUI?.SetEliminatedState(true);
            }
        }
        
        gameModeIndicator?.OnGameStateChanged();
        dropButton?.UpdateDropButtonVisibility();
    }
    #endregion
}
