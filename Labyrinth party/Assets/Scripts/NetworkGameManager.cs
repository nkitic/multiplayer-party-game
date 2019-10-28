using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class NetworkGameManager : MonoBehaviour
{

    private const short CLIENT_BASE_ID = 900;
    private const int MIN_NUM_OF_PLAYERS = 1;       
    private const int MAX_NUM_OF_PLAYERS = 4;

    private int port = 25000;
    private static IMessageManager gameManager;
    private static string ipaddress;

    private static int[] connectionId = new int[MAX_NUM_OF_PLAYERS];
    public static int playerCount;    

    // Use this for initialization
    void Start ()
    {
        DontDestroyOnLoad(this.gameObject);

        playerCount = 0;

        if (NetworkServer.Listen(port))
        {
            ipaddress = Network.player.ipAddress;

            UIManager.gameStatus = UIManager.GameStatus.ServerStarted;

            NetworkServer.RegisterHandler(MsgType.Connect, OnClientConnected);

            for(var player = 1; player <= MAX_NUM_OF_PLAYERS; player++)
                NetworkServer.RegisterHandler((Int16)(CLIENT_BASE_ID + player), ServerReciveMessage);
        }
        else
        {
            Debug.Log("Failed To Create Server!");
        }
    }

    void OnClientConnected(NetworkMessage message)
    {
        connectionId[playerCount] = message.conn.connectionId;

        SendMessageToClient(playerCount.ToString(), playerCount);

        playerCount++;
        gameManager.ProcessNetworkMessages("NewClientConneceted");

        if (playerCount == MIN_NUM_OF_PLAYERS)
        {
            UIManager.gameStatus = UIManager.GameStatus.MinimumPlayersConneceted;
        }
        else if (playerCount == MAX_NUM_OF_PLAYERS)
        {
            //NetworkServer.dontListen = true;          //TODO: stop server from listening on new client connection -> currently not working
            gameManager.ProcessNetworkMessages("StartGame");
        }
    }

    private void ServerReciveMessage(NetworkMessage message)
    {
        StringMessage msg = new StringMessage();
        msg.value = message.ReadMessage<StringMessage>().value;

        Debug.Log(msg.value);
        gameManager.ProcessNetworkMessages(msg.value);
    }

    public static void SendMessageToClient(string message, int playerId)
    {
        StringMessage msg = new StringMessage();
        msg.value = message;
        NetworkServer.SendToClient(connectionId[playerId], CLIENT_BASE_ID, msg);
    }

    public static void SendMessageToAllClients(string message)
    {
        for (var player = 0; player < playerCount; player++)
            SendMessageToClient(message, player);
    }

    public static string GetIpAddress()
    {
        return ipaddress;
    }

    public static void SetNewGameManager(IMessageManager newGameManager)
    {
        gameManager = newGameManager;
    }

    public static void StopServer()
    {
        NetworkServer.Shutdown();
    }
}
