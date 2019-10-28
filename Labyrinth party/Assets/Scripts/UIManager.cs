using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour, IMessageManager
{
    [SerializeField] Text serverIp;
    [SerializeField] Text serverStatus;
    [SerializeField] Button startButton;
    [SerializeField] Text playerCountText;

    public enum GameStatus { WaitingForServer, ServerStarted, WaitingForMinimumPlayers, MinimumPlayersConneceted, WaitingForStart };
    public static GameStatus gameStatus = GameStatus.WaitingForServer;

    // Use this for initialization
    void Start () {
        NetworkGameManager.SetNewGameManager(this);

        startButton.interactable = false;
    }
	
	// Update is called once per frame
	void Update () {
		if(gameStatus == GameStatus.ServerStarted)
        {
            ChangeUI();
            gameStatus = GameStatus.WaitingForMinimumPlayers;
        }
        else if(gameStatus == GameStatus.MinimumPlayersConneceted)
        {
            startButton.interactable = true;
            gameStatus = GameStatus.WaitingForStart;
        }
    }

    public void ProcessNetworkMessages(string message) 
    {
        if (message == "StartGame")
        {
            StartGame();
        }
        else if(message == "NewClientConneceted")
        {
            ChangePlayerCountText();
        }
    }

    public void StartGame()
    {
        NetworkGameManager.SendMessageToAllClients("StartGame");
        SceneManager.LoadScene(1);
    }

    private void ChangeUI()
    {
        serverIp.text = "Connect to IP: " +  NetworkGameManager.GetIpAddress();
        serverStatus.text = "Server is up!";
        //TODO: promijeniti player connected u broj igrača
    }

    private void ChangePlayerCountText()
    {
        playerCountText.text = "Players connected: " + NetworkGameManager.playerCount;
    }
}
