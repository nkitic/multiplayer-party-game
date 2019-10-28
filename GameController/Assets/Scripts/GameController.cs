using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class GameController : MonoBehaviour, IMessageManager
{
    private const float WAIT_SEC = 3f;
    private const int ADD_TILE_COST = 3;
    private const int MOVE_ONE_TILE_COST = 1;
    private const int MINIGAME_BUILD_INDEX_DIFF = 2;

    [SerializeField] Text playerName;
    [SerializeField] Image playerColor;
    [SerializeField] Text coinText;
    [SerializeField] Text diamondText;

    [SerializeField] Text waitForTurn;
    [SerializeField] GameObject onTurnGroup;

    [SerializeField] Button throwDiceButton;
    [SerializeField] Button addTileButton;
    [SerializeField] Button movePlayerButton;
    [SerializeField] Button doneButton;
    [SerializeField] Text notification;

    private int additiveScene = -1;
    private int numOfCoins;

    private Color[] playerColors = { Color.red, Color.blue, Color.green, Color.gray };
    
    // Use this for initialization
    void Start() {
        NetworkClientManager.SetNewGameManager(this);

        playerName.text = "Player" + (NetworkClientManager.clientId + 1);

        playerColor.color = playerColors[NetworkClientManager.clientId];

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public void ThrowDice()
    {
        NetworkClientManager.SendMessageToServer("ThrowDice");

        throwDiceButton.interactable = false;

        addTileButton.interactable = true;
        movePlayerButton.interactable = true;
        doneButton.interactable = true;
    }

    public void AddTile()
    {
        if(numOfCoins < ADD_TILE_COST)
        {
            StartCoroutine(ShowNotification("Not enough coins!"));
        }
        else
        {
            NetworkClientManager.SendMessageToServer("AddTile");

            addTileButton.interactable = false;
            LoadAdditiveScene(2);
        }
    }

    public void MovePlayer()
    {
        if (numOfCoins < MOVE_ONE_TILE_COST)
        {
            StartCoroutine(ShowNotification("Not enoguh coins!"));
        }
        else
        {
            NetworkClientManager.SendMessageToServer("MovePlayer");
            
            LoadAdditiveScene(3);
        }
    }

    public void TurnDone()
    {
        NetworkClientManager.SendMessageToServer("TurnDone");
    }

    public void ProcessNetworkMessages(string message)
    {
        if (message == "StartTurn")
        {
            StartTurn();
        }
        else if (message == "EndTurn")
        {
            EndTurn();
        }
        else if (message == "Back")
        {
            if (additiveScene != -1)
            {
                SceneManager.UnloadSceneAsync(additiveScene);
            }
            additiveScene = -1;
        }
        else if(message == "Loading")
        {
            LoadAdditiveScene(4);
        }
        else if (message == "Ready?")
        {
            LoadAdditiveScene(5);
        }
        else if (message == "TileOccupied")
        {
            StartCoroutine(ShowNotification("Tile already occupied!"));     
        }
        else if (message == "Restart")
        {
            Destroy(GameObject.Find("Network Manager"));
            SceneManager.LoadScene(0);
        }
        else
        {
            string[] messageParts = message.Split('|');

            if (messageParts[0] == "Coins")
            {
                numOfCoins = Int32.Parse(messageParts[1]);
                coinText.text = "Coins: " + numOfCoins;
            }
            else if (messageParts[0] == "Diamonds")
            {
                diamondText.text = "Diamonds: " + messageParts[1];
            }
            else if (messageParts[0] == "Minigame")
            {
                LoadAdditiveScene(Int32.Parse(messageParts[1]) + MINIGAME_BUILD_INDEX_DIFF);
            }
        }
    }

    private void StartTurn()
    {
        throwDiceButton.interactable = true;

        addTileButton.interactable = false;
        movePlayerButton.interactable = false;
        doneButton.interactable = false;

        onTurnGroup.SetActive(true);

        waitForTurn.gameObject.SetActive(false);
    }

    private void EndTurn()
    {
        waitForTurn.gameObject.SetActive(true);
        onTurnGroup.SetActive(false);
    }

    private void LoadAdditiveScene(int scene)
    {
        if (additiveScene != -1)
        {
            SceneManager.UnloadSceneAsync(additiveScene);
        }

        additiveScene = scene;
        SceneManager.LoadScene(additiveScene, LoadSceneMode.Additive);
    }

    private IEnumerator ShowNotification(string notificationText)
    {
        notification.text = notificationText;
        notification.gameObject.SetActive(true);

        yield return new WaitForSeconds(WAIT_SEC);

        notification.gameObject.SetActive(false);
    }
}
