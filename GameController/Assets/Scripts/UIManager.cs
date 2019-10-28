using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour, IMessageManager
{
    
    [SerializeField] Text connectionStatusText;
    [SerializeField] Text gameStatusText;
    [SerializeField] Text welcomeText;

    [SerializeField] InputField serverIp;
    [SerializeField] Button connectButton;

    // Use this for initialization
    void Start () {
        NetworkClientManager.SetNewGameManager(this);

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
	
	// Update is called once per frame
	void Update () {
    }

    private void ChangeUI()
    {
        connectionStatusText.text = "Connected to server!";
        connectionStatusText.color = Color.green;

        welcomeText.text = "Welcome Player" + (NetworkClientManager.clientId + 1) +"!";

        serverIp.enabled = false;
        connectButton.interactable = false;

        gameStatusText.text = "Waiting for other players!"; 
    }

    public void ProcessNetworkMessages(string message)
    {
        if (message == "StartGame")
        {
            StartGame();
        }
        else if (message == "Connected")
        {
            ChangeUI();
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
}
