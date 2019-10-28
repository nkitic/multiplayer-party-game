using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

public class NetworkClientManager : MonoBehaviour {

    private const short CLIENT_BASE_ID = 900;

    private static NetworkClient client;
    private string serverIp = "192.168.5.146";
    private int port = 25000;
    public static int clientId;

    private static IMessageManager gameManager;

    private bool successMessageSent = false;


    // Use this for initialization
    void Start ()
    {
        DontDestroyOnLoad(this.gameObject);

        client = new NetworkClient();
    }

    // Update is called once per frame
    void Update ()
    {
        if (client.isConnected && !successMessageSent)
        {
            successMessageSent = true;

            client.RegisterHandler(CLIENT_BASE_ID, ClientReceiveMessage);
        }
    }

    public void Connect()
    {
        serverIp = FindObjectOfType<InputField>().text;
        client.Connect(serverIp, port);
    }

    public static void SendMessageToServer(string message)
    {
        if(client.isConnected)
        {
            StringMessage msg = new StringMessage();
            msg.value = message;
            client.Send((Int16)(CLIENT_BASE_ID + clientId + 1), msg);
        }
    }

    public void ClientReceiveMessage(NetworkMessage message)
    {
        StringMessage msg = new StringMessage();
        msg.value = message.ReadMessage<StringMessage>().value;

        if(msg.value == "0" || msg.value == "1" || msg.value == "2" || msg.value == "3")
        {
            clientId = Int32.Parse(msg.value);
            gameManager.ProcessNetworkMessages("Connected");
        }
        else
        {
            gameManager.ProcessNetworkMessages(msg.value);
        }
        Debug.Log(msg.value);
    }

    public static void SetNewGameManager(IMessageManager newGameManager)
    {
        gameManager = newGameManager;
    }
}
