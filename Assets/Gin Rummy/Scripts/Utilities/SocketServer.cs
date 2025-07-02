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
    reshuffle
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

          try
          {
              // Extract JSON as an object (not an array)
              var matchData = response.ToString();


              if (matchData != null)
              {
                  // Serialize into JSON string for event handling
                  string jsonResponse = JsonUtility.ToJson(matchData);
                  OnFinalWinner?.Invoke(matchData);
              }
              else
              {
                  Debug.LogWarning("[Socket] final_winner: Match data is null.");
              }

          }
          catch (Exception ex)
          {
              Debug.LogError($"[Socket] Error processing final_winner event: {ex.Message}");
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

    void OnDestroy()
    {
        socket.Dispose();
    }

    public void Disconnect()
    {
        socket.Disconnect();
    }
}
