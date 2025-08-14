using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using static SocketIOUnity;

public enum RummySocketEvents
{
    player_ready,
    cards_received,
    request_card,
    take_card_from_deck,// not impl.
    drop_card,
    card_taken_from_discard,
    pass_turn,
    end_turn,
    deal_cards,
    start_turn,
    next_turn,
    shuffle_pile,
    pile_shuffled,
    card_fetched,
    card_taken_from_deck,
    card_dropped,
    card_taken_from_discard_confirm,
    turn_passed,
    new_deal,
    reset_game_state,
    final_winner,
    new_round,
    error,
    player_disconnected,
    end_game,
    game_ended,
    on_game_ended,
    match_cancelled,
    pick_card,
    update_player_hand,
    reshuffle,
    
    // 🔹 NEW ENHANCED GAME EVENTS
    player_dropped,              // Player drops from Pool Rummy
    player_eliminated,           // Player eliminated from Pool Rummy
    deal_completed,              // Deal completed in Deals Rummy
    deal_started,                // New deal started
    pool_game_ended,             // Pool game ended with winner
    cumulative_score_updated,    // Score update for tracking
    game_mode_changed,           // Game mode state change
    active_players_updated       // Active player count updated
}

public class RummySocketServer : SingletonWithGameobject<RummySocketServer>
{
    public UnityEvent<PlayerCardsReponce> OnDealCard=new  UnityEvent<PlayerCardsReponce>();
    public UnityEvent<FetchedCardData> OnCardFetched = new UnityEvent<FetchedCardData>();
    public UnityEvent<string> OnNewDeal = new UnityEvent<string>();
    public UnityEvent<string> OnNewRound = new UnityEvent<string>();
    public UnityEvent<PlayerDeck> OnReShuffle = new UnityEvent<PlayerDeck>();
    public UnityEvent<CardDropped> OnCardTakenFromDeck = new UnityEvent<CardDropped>();
    public UnityEvent<CardDropped> OnCardDropped = new UnityEvent<CardDropped>();
    public UnityEvent<CardDropped> OnCardTakenFromDiscard = new UnityEvent<CardDropped>();
    public UnityEvent OnTurnPassed = new UnityEvent();
    public UnityEvent OnGameEnded = new UnityEvent();
    public UnityEvent<string> OnPlayerDisconnected = new UnityEvent<string>();
    public UnityEvent<string> OnStartTurn = new UnityEvent<string>();
    public UnityEvent<string> OnNextTurn = new UnityEvent<string>();
    public UnityEvent<string> OnFinalWinner = new UnityEvent<string>();
    public UnityEvent OnResetGame = new UnityEvent();
    public UnityEvent<PileShuffled> OnShufflePile = new UnityEvent<PileShuffled>();
    public UnityEvent<Dictionary<string, string>> OnGameFinished = new UnityEvent<Dictionary<string, string>>();
    public UnityEvent<string> OnError = new UnityEvent<string>();
    
    // 🔹 NEW ENHANCED GAME EVENTS
    public UnityEvent<PlayerDroppedData> OnPlayerDropped = new UnityEvent<PlayerDroppedData>();
    public UnityEvent<PlayerEliminatedData> OnPlayerEliminated = new UnityEvent<PlayerEliminatedData>();
    public UnityEvent<DealCompletedData> OnDealCompleted = new UnityEvent<DealCompletedData>();
    public UnityEvent<DealStartedData> OnDealStarted = new UnityEvent<DealStartedData>();
    public UnityEvent<PoolGameEndedData> OnPoolGameEnded = new UnityEvent<PoolGameEndedData>();
    public UnityEvent<CumulativeScoreData> OnCumulativeScoreUpdated = new UnityEvent<CumulativeScoreData>();
    public UnityEvent<GameModeChangedData> OnGameModeChanged = new UnityEvent<GameModeChangedData>();
    public UnityEvent<ActivePlayersData> OnActivePlayersUpdated = new UnityEvent<ActivePlayersData>();

    private SocketIOUnity socket;
    [SerializeField] private string serverUrlLink;

    public void Initialize(string url)
    {
        serverUrlLink = url;
    }

    public async Task ConnectServer(string matchId)
    {
        var uri = new Uri(serverUrlLink);
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
            {
                {"playerId", UserDataContext.Instance.UserData._id },
                {"matchId", matchId }
            },
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        socket.JsonSerializer = new NewtonsoftJsonSerializer();
        socket.unityThreadScope = UnityThreadScope.Update;

        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("Game Server Connected");

        };
        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.deal_cards), response =>
        {
            try
            {
                Debug.Log($"[Socket] Received: deal_cards - <color=green>{response}</color>");
                var responseParsed = response.GetValue<PlayerCardsReponce>();
                Debug.Log("Invoking OnDealCard...");
                OnDealCard?.Invoke(responseParsed);
                Debug.Log("OnDealCard invoked.");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        });

        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.card_fetched), response =>
        {
            try
            {
                Debug.Log($"[Socket] Received: card_fetched - <color=green>{response}</color>");
                var responseParsed = response.GetValue<FetchedCardData>();
                OnCardFetched?.Invoke(responseParsed);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        });
        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.reshuffle), response =>
        {
            try
            {
                Debug.Log($"[Socket] Received: reshuffle \r\r - <color=green>{response}</color>");
                var responseParsed = response.GetValue<PlayerDeck>();
                OnReShuffle?.Invoke(responseParsed);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        });

        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.card_taken_from_deck), response =>
        {
            try
            {
                Debug.Log($"[Socket] Received: card_taken_from_deck - <color=green>{response}</color>");
                var responseParsed = response.GetValue<CardDropped>();
                OnCardTakenFromDeck?.Invoke(responseParsed);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        });

        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.card_dropped), response =>
        {
            try
            {
                Debug.Log($"[Socket] Received: card_dropped - <color=green>{response}</color>");
                var responseParsed = response.GetValue<CardDropped>();
                OnCardDropped?.Invoke(responseParsed);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        });

        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.card_taken_from_discard_confirm), response =>
        {
            try
            {
                Debug.Log($"[Socket] Received: card_taken_from_discard_confirm - <color=green>{response}</color>");
                var responseParsed = response.GetValue<CardDropped>();
                OnCardTakenFromDiscard?.Invoke(responseParsed);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        });

        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.shuffle_pile), response =>
        {
            try
            {
                Debug.Log($"[Socket] Received: shuffle_pile - <color=green>{response}</color>");
                var responseParsed = response.GetValue<PileShuffled>();
                OnShufflePile?.Invoke(responseParsed);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        });

        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.turn_passed), response =>
        {
            Debug.Log("[Socket] Received: turn_passed");
            OnTurnPassed?.Invoke();
        });

        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.on_game_ended), response =>
        {
            Debug.Log("[Socket] Received: on_game_ended");
            OnGameEnded?.Invoke();
        });

        // 🔹 MISSING: Player Ready Response Handler
        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.player_ready), response =>
        {
            try
            {
                Debug.Log($"[Socket] Received: player_ready response - <color=green>{response}</color>");
                // Parse the response to get next game event
                var responseParsed = response.GetValue<Dictionary<string, object>>();
                
                if (responseParsed.ContainsKey("nextEvent"))
                {
                    string nextEvent = responseParsed["nextEvent"].ToString();
                    Debug.Log($"[Socket] Next event after player_ready: {nextEvent}");
                    
                    // Handle the next event based on what backend says
                    HandlePlayerReadyResponse(nextEvent, responseParsed);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to handle player_ready response: {e.Message}");
            }
        });

        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.start_turn), response =>
        {
            Debug.Log($"[Socket] Received: start_turn - <color=green>{response}</color>");
            var responseParsed = response.GetValue<Dictionary<string, string>>();
            OnStartTurn?.Invoke(responseParsed["playerId"]);
        });

        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.next_turn), response =>
        {
            Debug.Log($"[Socket] Received: next_turn - <color=green>{response}</color>");
            var responseParsed = response.GetValue<Dictionary<string, string>>();
            OnNextTurn?.Invoke(responseParsed["playerId"]);
        });

        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.player_disconnected), response =>
        {
            Debug.Log($"[Socket] Received: player_disconnected - <color=green>{response}</color>");
            var responseParsed = response.GetValue<Dictionary<string, string>>();
            OnPlayerDisconnected?.Invoke(responseParsed["playerId"]);
        });

        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.end_game), response =>
        {
            Debug.Log($"[Socket] Received: end_game - <color=green>{response}</color>");
            var responseParsed = response.GetValue<Dictionary<string, string>>();
            OnGameFinished?.Invoke(responseParsed);
        });

        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.new_deal), response =>
        {
            Debug.Log($"[Socket] new_deal Received: error - <color=green>{response}</color>");
            var responseParsed = response.ToString();

            // Convert Dictionary to JSON string for better handling


            if (responseParsed != null)
            {
                // Serialize into JSON string for event handling
                string jsonResponse = JsonUtility.ToJson(responseParsed);
                OnNewDeal?.Invoke(responseParsed);
            }
            else
            {
                Debug.LogWarning("[Socket] final_winner: Match data is null.");
            }
        });

        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.reset_game_state), response =>
        {
            Debug.Log($"[Socket] reset_game_state Received: error - <color=green>{response}</color>");
            OnResetGame.Invoke();
        });
        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.final_winner), response =>
      {
          Debug.Log($"[Socket] final_winner Received: error - <color=green>{response}</color>");
          var responseParsed = response.GetValue<Dictionary<string, string>>();


          if (responseParsed != null)
          {
              OnFinalWinner?.Invoke(responseParsed["winnerId"]);
          }
          else
          {
              Debug.LogWarning("[Socket] final_winner: Match data is null.");
          }
      });
        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.new_round), response =>
      {
          var NewRound = response.ToString();


          if (NewRound != null)
          {
              // Serialize into JSON string for event handling
              string jsonResponse = JsonUtility.ToJson(NewRound);
              OnNewRound?.Invoke(NewRound);
          }
          else
          {
              Debug.LogWarning("[Socket] final_winner: Match data is null.");
          }
    
      });

        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.error), response =>
        {
            Debug.Log($"[Socket] Received: error - <color=green>{response}</color>");
            var responseParsed = response.GetValue<Dictionary<string, string>>();
            OnError?.Invoke(responseParsed["message"]);
        });

        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log($"Game Server Disconnected: <color=red>{sender} {e}</color>");
        };


        await socket.ConnectAsync();
        
        // 🔹 NEW ENHANCED GAME EVENT HANDLERS
        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.player_dropped), response =>
        {
            try
            {
                Debug.Log($"[Socket] Received: player_dropped - <color=green>{response}</color>");
                var responseParsed = response.GetValue<PlayerDroppedData>();
                OnPlayerDropped?.Invoke(responseParsed);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Socket] Error handling player_dropped: {e}");
            }
        });

        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.player_eliminated), response =>
        {
            try
            {
                Debug.Log($"[Socket] Received: player_eliminated - <color=green>{response}</color>");
                var responseParsed = response.GetValue<PlayerEliminatedData>();
                OnPlayerEliminated?.Invoke(responseParsed);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Socket] Error handling player_eliminated: {e}");
            }
        });

        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.deal_completed), response =>
        {
            try
            {
                Debug.Log($"[Socket] Received: deal_completed - <color=green>{response}</color>");
                var responseParsed = response.GetValue<DealCompletedData>();
                OnDealCompleted?.Invoke(responseParsed);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Socket] Error handling deal_completed: {e}");
            }
        });

        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.deal_started), response =>
        {
            try
            {
                Debug.Log($"[Socket] Received: deal_started - <color=green>{response}</color>");
                var responseParsed = response.GetValue<DealStartedData>();
                OnDealStarted?.Invoke(responseParsed);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Socket] Error handling deal_started: {e}");
            }
        });

        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.pool_game_ended), response =>
        {
            try
            {
                Debug.Log($"[Socket] Received: pool_game_ended - <color=green>{response}</color>");
                var responseParsed = response.GetValue<PoolGameEndedData>();
                OnPoolGameEnded?.Invoke(responseParsed);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Socket] Error handling pool_game_ended: {e}");
            }
        });

        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.cumulative_score_updated), response =>
        {
            try
            {
                Debug.Log($"[Socket] Received: cumulative_score_updated - <color=green>{response}</color>");
                var responseParsed = response.GetValue<CumulativeScoreData>();
                OnCumulativeScoreUpdated?.Invoke(responseParsed);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Socket] Error handling cumulative_score_updated: {e}");
            }
        });

        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.active_players_updated), response =>
        {
            try
            {
                Debug.Log($"[Socket] Received: active_players_updated - <color=green>{response}</color>");
                var responseParsed = response.GetValue<ActivePlayersData>();
                OnActivePlayersUpdated?.Invoke(responseParsed);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Socket] Error handling active_players_updated: {e}");
            }
        });
    }
    [System.Serializable]
    public class MatchWinnerData
    {
        public string winner;
        public Dictionary<string, int> matchWins;
    }
    // Wrapper class for JSON serialization
    [System.Serializable]
    public class Wrapper<T>
    {
        public T data;
    }
    public async Task SendEvent(RummySocketEvents eventName, Dictionary<string, string> data = null)
    {
        // Print event name and data before emitting
        Debug.Log($"Sending Event: {eventName}");

        if (data != null)
        {
            foreach (var kvp in data)
            {
                Debug.Log($"Key: eventName {eventName} <color=green>{kvp.Key}, Value: {kvp.Value}</color>");
            }
        }
        else
        {
            Debug.Log("Data is null");
        }

        await socket.EmitAsync(Enum.GetName(typeof(RummySocketEvents), eventName), data);
    }
    
    // 🔹 NEW ENHANCED EVENT SENDING METHOD FOR COMPLEX DATA
    public async Task SendEnhancedEvent<T>(RummySocketEvents eventName, T data) where T : class
    {
        try
        {
            Debug.Log($"Sending Enhanced Event: {eventName}");
            
            if (data != null)
            {
                string jsonData = JsonConvert.SerializeObject(data);
                Debug.Log($"Event data: <color=green>{jsonData}</color>");
            }
            else
            {
                Debug.Log("Enhanced event data is null");
            }

            await socket.EmitAsync(Enum.GetName(typeof(RummySocketEvents), eventName), data);
            Debug.Log($"Successfully sent enhanced event: {eventName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send enhanced event {eventName}: {e.Message}");
            throw;
        }
    }

    // 🔹 MISSING: Handle Player Ready Response Method
    private void HandlePlayerReadyResponse(string nextEvent, Dictionary<string, object> responseData)
    {
        try
        {
            Debug.Log($"[Socket] Handling player_ready response with next event: {nextEvent}");
            
            switch (nextEvent.ToLower())
            {
                case "start_game":
                case "game_start":
                    Debug.Log("[Socket] Game start event triggered from player_ready");
                    // Trigger game start logic
                    OnGameStart?.Invoke();
                    break;
                    
                case "start_turn":
                    if (responseData.ContainsKey("playerId"))
                    {
                        string playerId = responseData["playerId"].ToString();
                        Debug.Log($"[Socket] Start turn for player: {playerId}");
                        OnStartTurn?.Invoke(playerId);
                    }
                    break;
                    
                case "next_turn":
                    if (responseData.ContainsKey("playerId"))
                    {
                        string playerId = responseData["playerId"].ToString();
                        Debug.Log($"[Socket] Next turn for player: {playerId}");
                        OnNextTurn?.Invoke(playerId);
                    }
                    break;
                    
                case "deal_cards":
                case "card_distribution":
                    Debug.Log("[Socket] Card distribution event triggered");
                    // Handle card distribution if needed
                    break;
                    
                case "wait_for_players":
                    Debug.Log("[Socket] Waiting for more players");
                    // Handle waiting state
                    break;
                    
                default:
                    Debug.LogWarning($"[Socket] Unknown next event after player_ready: {nextEvent}");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Socket] Error handling player_ready response: {e.Message}");
        }
    }

    void OnDestroy()
    {
        socket.Dispose();
    }

    public void Disconnect()
    {
        socket.Disconnect();
    }
}
