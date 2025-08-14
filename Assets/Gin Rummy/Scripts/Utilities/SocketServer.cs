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
            Debug.Log("✅ [CONNECTION] Game Server Connected successfully");
        };

        // 🔧 FIX: Add connection error handler
        socket.OnUnityThread("connection_error", response =>
        {
            try
            {
                Debug.LogError($"❌ [CONNECTION ERROR] Backend connection error received: {response}");
                
                var errorData = response.GetValue<Dictionary<string, object>>();
                if (errorData != null)
                {
                    string errorMessage = errorData.ContainsKey("error") ? errorData["error"].ToString() : "Unknown connection error";
                    Debug.LogError($"❌ [CONNECTION ERROR] Error: {errorMessage}");
                    
                    if (errorData.ContainsKey("suggestion"))
                    {
                        Debug.LogWarning($"💡 [CONNECTION ERROR] Suggestion: {errorData["suggestion"]}");
                    }
                    
                    // Notify other systems about connection error
                    OnError?.Invoke(errorMessage);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ [CONNECTION ERROR] Failed to handle connection error: {e.Message}");
            }
        });
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

        // 🔹 ENHANCED: Player Ready Response Handler (fixes backend communication issue)
        socket.OnUnityThread(Enum.GetName(typeof(RummySocketEvents), RummySocketEvents.player_ready), response =>
        {
            try
            {
                Debug.Log($"🔍 [FRONTEND RESPONSE DEBUG] ===== PLAYER_READY RESPONSE RECEIVED =====");
                Debug.Log($"🔍 [FRONTEND RESPONSE DEBUG] Response received: <color=green>{response}</color>");
                Debug.Log($"🔍 [FRONTEND RESPONSE DEBUG] Response type: {response?.GetType().Name}");
                Debug.Log($"🔍 [FRONTEND RESPONSE DEBUG] Response is null: {response == null}");
                
                // Try multiple response formats to handle different backend responses
                if (response == null)
                {
                    Debug.LogError("❌ [FRONTEND RESPONSE DEBUG] player_ready response is null - backend not responding!");
                    return;
                }
                
                // Handle different response types
                if (response.GetValue<string>() != null)
                {
                    // Simple string response
                    string stringResponse = response.GetValue<string>();
                    Debug.Log($"[Socket] player_ready string response: {stringResponse}");
                    HandlePlayerReadyStringResponse(stringResponse);
                    return;
                }
                
                // Try dictionary response
                var responseParsed = response.GetValue<Dictionary<string, object>>();
                if (responseParsed != null)
                {
                    Debug.Log($"[Socket] player_ready dictionary response with {responseParsed.Count} keys");
                    
                    // Log all keys in the response for debugging
                    foreach (var kvp in responseParsed)
                    {
                        Debug.Log($"[Socket] Response key: {kvp.Key} = {kvp.Value}");
                    }
                    
                    // Check for next event
                    if (responseParsed.ContainsKey("nextEvent"))
                    {
                        string nextEvent = responseParsed["nextEvent"].ToString();
                        Debug.Log($"[Socket] ✅ Next event after player_ready: {nextEvent}");
                        HandlePlayerReadyResponse(nextEvent, responseParsed);
                    }
                    else if (responseParsed.ContainsKey("status"))
                    {
                        string status = responseParsed["status"].ToString();
                        Debug.Log($"[Socket] player_ready status: {status}");
                        HandlePlayerReadyStatusResponse(status, responseParsed);
                    }
                    else
                    {
                        Debug.Log("[Socket] ✅ player_ready response received but no specific next action found");
                        // Backend acknowledged the player_ready, this should prevent player removal
                    }
                }
                else
                {
                    Debug.Log("[Socket] ✅ player_ready response received (simple acknowledgment)");
                    // Even a simple response means backend got the event
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Socket] ❌ Failed to handle player_ready response: {e.Message}");
                Debug.LogError($"[Socket] Exception details: {e.StackTrace}");
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
            Debug.Log($"🔍 [SOCKET DEBUG] ===== SOCKET SEND ENHANCED EVENT =====");
            Debug.Log($"🔍 [SOCKET DEBUG] Event Name: {eventName}");
            Debug.Log($"🔍 [SOCKET DEBUG] Event Name String: {Enum.GetName(typeof(RummySocketEvents), eventName)}");
            Debug.Log($"🔍 [SOCKET DEBUG] Data Type: {typeof(T).Name}");
            Debug.Log($"🔍 [SOCKET DEBUG] Socket null check: {socket == null}");
            Debug.Log($"🔍 [SOCKET DEBUG] Socket connected: {socket?.Connected}");
            
            if (data != null)
            {
                string jsonData = JsonConvert.SerializeObject(data);
                Debug.Log($"🔍 [SOCKET DEBUG] JSON Data: <color=green>{jsonData}</color>");
                Debug.Log($"🔍 [SOCKET DEBUG] JSON Data Length: {jsonData.Length} characters");
            }
            else
            {
                Debug.LogError("❌ [SOCKET DEBUG] Enhanced event data is null!");
                return;
            }

            Debug.Log($"🔍 [SOCKET DEBUG] About to call socket.EmitAsync...");
            Debug.Log($"🔍 [SOCKET DEBUG] Socket namespace: {socket?.Options?.Path}");
            Debug.Log($"🔍 [SOCKET DEBUG] Socket URI: {socket?.ServerUri}");
            
            await socket.EmitAsync(Enum.GetName(typeof(RummySocketEvents), eventName), data);
            Debug.Log($"✅ [SOCKET DEBUG] socket.EmitAsync completed successfully for: {eventName}");
            Debug.Log($"🔍 [SOCKET DEBUG] Now waiting for backend response...");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ [SOCKET DEBUG] Failed to send enhanced event {eventName}: {e.Message}");
            Debug.LogError($"❌ [SOCKET DEBUG] Exception type: {e.GetType().Name}");
            Debug.LogError($"❌ [SOCKET DEBUG] Stack trace: {e.StackTrace}");
            throw;
        }
    }

    // 🔹 ENHANCED: Handle Player Ready Response Method
    private void HandlePlayerReadyResponse(string nextEvent, Dictionary<string, object> responseData)
    {
        try
        {
            Debug.Log($"[Socket] ✅ Handling player_ready response with next event: {nextEvent}");
            
            switch (nextEvent.ToLower())
            {
                case "start_game":
                case "game_start":
                case "begin_game":
                    Debug.Log("[Socket] 🎮 Game start event triggered from player_ready");
                    //OnGameStart?.Invoke();
                    break;
                    
                case "start_turn":
                case "begin_turn":
                    if (responseData.ContainsKey("playerId"))
                    {
                        string playerId = responseData["playerId"].ToString();
                        Debug.Log($"[Socket] 🎯 Start turn for player: {playerId}");
                        OnStartTurn?.Invoke(playerId);
                    }
                    else
                    {
                        Debug.LogWarning("[Socket] start_turn response missing playerId");
                    }
                    break;
                    
                case "next_turn":
                    if (responseData.ContainsKey("playerId"))
                    {
                        string playerId = responseData["playerId"].ToString();
                        Debug.Log($"[Socket] ➡️ Next turn for player: {playerId}");
                        OnNextTurn?.Invoke(playerId);
                    }
                    else
                    {
                        Debug.LogWarning("[Socket] next_turn response missing playerId");
                    }
                    break;
                    
                case "deal_cards":
                case "card_distribution":
                case "distribute_cards":
                    Debug.Log("[Socket] 🃏 Card distribution event triggered");
                    // Trigger card distribution
                    //OnGameStart?.Invoke();
                    break;
                    
                case "wait_for_players":
                case "waiting":
                    Debug.Log("[Socket] ⏳ Waiting for more players");
                    // Continue waiting - no immediate action needed
                    break;
                    
                case "match_ready":
                case "all_players_ready":
                    Debug.Log("[Socket] ✅ All players ready - starting match");
                    //OnGameStart?.Invoke();
                    break;
                    
                default:
                    Debug.LogWarning($"[Socket] ⚠️ Unknown next event after player_ready: {nextEvent}");
                    // Still acknowledge that response was received
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Socket] ❌ Error handling player_ready response: {e.Message}");
        }
    }
    
    // 🔹 NEW: Handle string response for player_ready
    private void HandlePlayerReadyStringResponse(string response)
    {
        try
        {
            Debug.Log($"[Socket] ✅ Handling player_ready string response: {response}");
            
            switch (response.ToLower())
            {
                case "acknowledged":
                case "received":
                case "ok":
                case "success":
                    Debug.Log("[Socket] ✅ Player ready acknowledged by backend");
                    break;
                    
                case "start_game":
                case "begin":
                    Debug.Log("[Socket] 🎮 Starting game from string response");
                    //OnGameStart?.Invoke();
                    break;
                    
                case "wait":
                case "waiting":
                    Debug.Log("[Socket] ⏳ Backend says to wait for more players");
                    break;
                    
                default:
                    Debug.Log($"[Socket] 📝 Unknown string response: {response}");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Socket] ❌ Error handling string response: {e.Message}");
        }
    }
    
    // 🔹 NEW: Handle status response for player_ready
    private void HandlePlayerReadyStatusResponse(string status, Dictionary<string, object> responseData)
    {
        try
        {
            Debug.Log($"[Socket] ✅ Handling player_ready status response: {status}");
            
            switch (status.ToLower())
            {
                case "ready":
                case "accepted":
                case "confirmed":
                    Debug.Log("[Socket] ✅ Player ready status confirmed");
                    
                    // Check if there are additional instructions
                    if (responseData.ContainsKey("action"))
                    {
                        string action = responseData["action"].ToString();
                        Debug.Log($"[Socket] Action to take: {action}");
                        HandlePlayerReadyResponse(action, responseData);
                    }
                    break;
                    
                case "waiting":
                case "pending":
                    Debug.Log("[Socket] ⏳ Player ready but waiting for others");
                    break;
                    
                case "rejected":
                case "error":
                    Debug.LogWarning("[Socket] ❌ Player ready was rejected!");
                    if (responseData.ContainsKey("reason"))
                    {
                        Debug.LogWarning($"[Socket] Rejection reason: {responseData["reason"]}");
                    }
                    break;
                    
                default:
                    Debug.Log($"[Socket] 📝 Unknown status: {status}");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Socket] ❌ Error handling status response: {e.Message}");
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
