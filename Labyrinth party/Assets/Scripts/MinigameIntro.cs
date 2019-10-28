using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinigameIntro : MonoBehaviour, IMessageManager
{
    private const int MIN_MINIGAME_BUILD_NUM = 5;
    private const int MAX_MINIGAME_BUILD_NUM = 6;

    [SerializeField] GameObject[] playersReady;
    [SerializeField] Text title;
    [SerializeField] Text description;

    private int playersReadyCount = 0;

    // Use this for initialization
    void Start() {
        GameController.SetNewMinigameManager(this);

        ChooseRandomMinigame();
        InitializePlayersReady();
        SetupMinigameTitleAndDescription();

        NetworkGameManager.SendMessageToAllClients("Ready?");
    }

    public void ProcessNetworkMessages(string message)
    {
        string[] messageParts = message.Split('|');

        if (messageParts[0] == "Ready")
        {
            MarkPlayerAsReady(System.Int32.Parse(messageParts[1]));

            playersReadyCount++;

            if(playersReadyCount == NetworkGameManager.playerCount)
            {
                LoadMinigame();
            }
        }
    }

    private void ChooseRandomMinigame()
    {
        GameController.minigameId = Random.Range(MIN_MINIGAME_BUILD_NUM, MAX_MINIGAME_BUILD_NUM);
    }

    private void SetupMinigameTitleAndDescription()
    {

        // TODO: pull text from database or array

        title.text = "Timer";
        description.text = "Stop time at specified time.";
    }

    private void InitializePlayersReady()
    {
        for(var playerId = 0; playerId < NetworkGameManager.playerCount; playerId++)
        {
            playersReady[playerId].gameObject.SetActive(true);            
        }
    }

    private void MarkPlayerAsReady(int playerId)
    {
        playersReady[playerId].GetComponent<Image>().color = Color.green;
    }

    private void LoadMinigame()
    {
        NetworkGameManager.SendMessageToAllClients("Loading");
        SceneManager.UnloadSceneAsync(2);
        SceneManager.LoadScene(GameController.minigameId, LoadSceneMode.Additive);
    }
}
