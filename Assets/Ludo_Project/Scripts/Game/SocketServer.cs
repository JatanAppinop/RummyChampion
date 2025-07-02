using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using static SocketIOUnity;

public class SocketServer : SingletonWithGameobject<SocketServer>
{
    public UnityEvent<string> onStartMatch = new UnityEvent<string>();
    public UnityEvent<string> onPlayerDisconnected = new UnityEvent<string>();
    public UnityEvent<Dictionary<string, string>> onDiceClicked = new UnityEvent<Dictionary<string, string>>();
    public UnityEvent<Dictionary<string, string>> onDiceRolled = new UnityEvent<Dictionary<string, string>>();
    public UnityEvent<Dictionary<string, string>> onTokenMoved = new UnityEvent<Dictionary<string, string>>();
    public UnityEvent<Dictionary<string, string>> onGameFinished = new UnityEvent<Dictionary<string, string>>();
    public UnityEvent<Dictionary<string, string>> onEmojiReaction = new UnityEvent<Dictionary<string, string>>();
    public UnityEvent<string> onTurnMissed = new UnityEvent<string>();
    public UnityEvent<string> onNextTurn = new UnityEvent<string>();

    private SocketIOUnity socket;
    [SerializeField] private string serverUrlLink;

    public void Initialize(string url)
    {
        serverUrlLink = url;
    }
    public enum LUDOEVENT
    {
        start_match,
        player_disconnected,
        Dice_Clicked,
        Dice_Rolled,
        MoveToken,
        NextTurn, 
        Game_Finished,
        Turn_Missed,
        Emoji_Reaction

    }

    public async Task ConnectServer(string matchId)
    {

        var uri = new Uri(serverUrlLink);
        // Debug.Log("Game Server : " + serverUrlLink);
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
            {
                {"playerId", UserDataContext.Instance.UserData._id },
                {"matchId", matchId }
            }
    ,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        socket.JsonSerializer = new NewtonsoftJsonSerializer();
        socket.unityThreadScope = UnityThreadScope.Update;

        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("<color=green>Game Server Connected</color>");

        };

        socket.OnUnityThread("start_match", response =>
        {
            Debug.Log("Start Match : " + response.ToString());
            var responParsed = response.GetValue<Dictionary<string, string>>();
            Debug.Log($"<color=green>Event start_match :  +{responParsed.ToString()}</color> ");
            onStartMatch?.Invoke(responParsed["playerId"]);
        });


        socket.OnUnityThread("player_disconnected", response =>
        {
            var responParsed = response.GetValue<Dictionary<string, string>>();
            Debug.Log("Player Disconnected : " + response);
            Debug.Log($"<color=red>Event player_disconnected :  +{responParsed.ToString()}</color> ");
            onPlayerDisconnected?.Invoke(responParsed["playerId"]);
        });

        socket.OnUnityThread("Dice_Clicked", response =>
        {
            var responParsed = response.GetValue<Dictionary<string, string>>();
            Debug.Log($"<color=green>Event Dice_Clicked :  +{responParsed.ToString()}</color> ");
            onDiceClicked?.Invoke(responParsed);
        });
        socket.OnUnityThread("Dice_Rolled", response =>
        {
            var responParsed = response.GetValue<Dictionary<string, string>>();
            Debug.Log($"<color=green>Event Dice_Rolled :  +{responParsed.ToString()}</color> ");
            onDiceRolled?.Invoke(responParsed);
        });
        socket.OnUnityThread("MoveToken", response =>
        {
            var responParsed = response.GetValue<Dictionary<string, string>>();
            Debug.Log($"<color=green>Event MoveToken :  +{responParsed.ToString()}</color> ");
            onTokenMoved?.Invoke(responParsed);
        });
        socket.OnUnityThread("NextTurn", response =>
        {
            var responParsed = response.GetValue<Dictionary<string, string>>();
            Debug.Log("Next Turn Invoked");
            Debug.Log($"<color=green>Event NextTurn :  +{responParsed.ToString()}</color> ");
            onNextTurn?.Invoke(responParsed["playerId"]);
        });
        socket.OnUnityThread("Game_Finished", response =>
        {
            var responParsed = response.GetValue<Dictionary<string, string>>();
            Debug.Log($"<color=green>Event Game_Finished :  +{responParsed.ToString()}</color> ");
            onGameFinished?.Invoke(responParsed);
        });
        socket.OnUnityThread("Turn_Missed", response =>
        {
            var responParsed = response.GetValue<Dictionary<string, string>>();
            Debug.Log($"<color=green>Event Turn_Missed :  +{responParsed.ToString()}</color> ");
            onTurnMissed?.Invoke(responParsed["playerId"]);
        });
        socket.OnUnityThread("Emoji_Reaction", response =>
        {
            var responParsed = response.GetValue<Dictionary<string, string>>();
            onEmojiReaction?.Invoke(responParsed);
        });
        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log($"<color=red>Game Server Disconnected :  +{sender.ToString()} {e.ToString()}</color> ");

        };

        await socket.ConnectAsync();

    }

    public async Task SendEvent(string eventName, Dictionary<string, string> data = null)
    {
        await socket.EmitAsync(eventName, data);
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