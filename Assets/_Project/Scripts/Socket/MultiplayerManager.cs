using System;
using System.Collections.Generic;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using TMPro;
using System.Collections;
using UnityEngine.Events;
using static SocketIOUnity;

public class MultiplayerManager : MonoBehaviour
{
    private SocketIOUnity socket;
    [SerializeField] private string serverUrlLink;

    public UnityEvent<string> onMatchFound = new UnityEvent<string>();

    public void ConnectServer(string url, string contestId)
    {
        serverUrlLink = url;
        var uri = new Uri(serverUrlLink);
        //socket = new SocketIOUnity(uri);
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
            {
                {"playerId", UserDataContext.Instance.UserData._id },
                {"contestId", contestId }
            }
    ,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
        });

        socket.JsonSerializer = new NewtonsoftJsonSerializer();
        socket.unityThreadScope = UnityThreadScope.Update;


        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("Match Making server Connected");

        };

        socket.OnError += (sender, e) =>
        {
            Debug.LogError(e);
        };

        socket.OnUnityThread("match_found", response =>
        {
            var responParsed = response.GetValue<Dictionary<string, string>>();

            onMatchFound?.Invoke(responParsed["matchId"]);
        });

        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("Match Making server Disconnected : " + sender.ToString() + e.ToString());

        };

        socket.Connect();

    }


    void OnDestroy()
    {
        //socket.Dispose();
    }

    public void Disconnect()
    {
        socket.Disconnect();
    }
}
