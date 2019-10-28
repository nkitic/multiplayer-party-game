using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class EndGameController : MonoBehaviour
{
    private const float WAIT_SEC = 2f;
    private const float SPACE_BETWEEN_RESULTS = 100f;
    private Vector3 resultOffset;

    [SerializeField] Transform canvasTransform;
    [SerializeField] GameObject resultPrefab;

	// Use this for initialization
	void Start () {

        resultOffset = new Vector3(Screen.width / 2, Screen.height / 3, 0f);
        //ShowResults();
        ShowWinner();
        StartCoroutine(TriggerRestartGame());
    }

    private void ShowWinner()
    {
        var transformOffset = new Vector3(0f, 0f, 0f);

        InstantiateResult(GameController.winner, transformOffset);
    }

    private void ShowResults()              //if we want to show results of all players
    {
        var transformOffset = new Vector3(0f, 0f, 0f);

        if (NetworkGameManager.playerCount == 2)
        {
            transformOffset.x = -SPACE_BETWEEN_RESULTS / 2;
        }
        else if (NetworkGameManager.playerCount == 3)
        {
            transformOffset.x = -SPACE_BETWEEN_RESULTS;
        }
        else if (NetworkGameManager.playerCount == 4)
        {
            transformOffset.x = -3 * SPACE_BETWEEN_RESULTS / 2;
        }

        for (var playerId = 0; playerId < NetworkGameManager.playerCount; playerId++)
        {
            InstantiateResult(playerId, transformOffset);
            transformOffset.x += SPACE_BETWEEN_RESULTS;
        }
    }

    private void InstantiateResult(int playerId, Vector3 transformOffset)
    {
        transformOffset += resultOffset;
        var resultObject = Instantiate(resultPrefab, transformOffset, Quaternion.identity, canvasTransform);

        //var resultText = resultObject.transform.Find("Result Text").gameObject.GetComponent<Text>();      //if we want to show results of all players
        //resultText.text = (GameController.gameResults[playerId] + 1) + ". Player" + (playerId + 1);

        var resultText = resultObject.transform.Find("Result Text").gameObject.GetComponent<Text>();
        resultText.text = "Player" + (playerId + 1);

        var playerImage = resultObject.transform.Find("Player Image").gameObject.GetComponent<Image>();
        playerImage.color = PlayingAreaController.players[playerId].GetComponent<Renderer>().material.color;

        var coinText = resultObject.transform.Find("Coins Text").gameObject.GetComponent<Text>();
        coinText.text = "Diamonds: " + PlayingAreaController.players[playerId].GetComponent<Player>().GetNumOfDiamonds() +
            "\nCoins: " + PlayingAreaController.players[playerId].GetComponent<Player>().GetNumOfCoins();
    }

    private IEnumerator TriggerRestartGame()
    {
        yield return new WaitForSeconds(WAIT_SEC);

        RestartPlayers();

        yield return new WaitForSeconds(WAIT_SEC);

        RestartGame();        
    }

    private void RestartGame()
    {
        NetworkGameManager.StopServer();
        Destroy(GameObject.Find("Network Manager"));

        SceneManager.LoadScene(0);
    }

    private void RestartPlayers()
    {
        NetworkGameManager.SendMessageToAllClients("Restart");
    }
}
