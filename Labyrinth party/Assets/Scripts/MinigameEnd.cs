using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinigameEnd : MonoBehaviour
{
    private const float WAIT_SEC = 5f;
    private const float SPACE_BETWEEN_RESULTS = 100f;
    private Vector3 resultOffset;

    [SerializeField] Transform canvasTransform;
    [SerializeField] GameObject resultPrefab;

    // Use this for initialization
    void Start () {
        resultOffset = new Vector3(Screen.width / 2, Screen.height / 3, 0f);
        ShowResults();

        StartCoroutine(BackToMainGame());
    }

    private void ShowResults()
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

        var resultText = resultObject.transform.Find("Result Text").gameObject.GetComponent<Text>();
        resultText.text = (GameController.minigameResults[playerId] + 1) + ".";

        var playerImage = resultObject.transform.Find("Player Image").gameObject.GetComponent<Image>();
        playerImage.color = PlayingAreaController.players[playerId].GetComponent<Renderer>().material.color;

        var coinText = resultObject.transform.Find("Coins Text").gameObject.GetComponent<Text>();
        coinText.text = "+" + GameController.minigameRewards[GameController.minigameResults[playerId]];
    }

    private IEnumerator BackToMainGame()
    {
        yield return new WaitForSeconds(WAIT_SEC);

        SceneManager.UnloadSceneAsync(3);
        NetworkGameManager.SendMessageToAllClients("Back");
        GameController.EndMinigame();
    }
}
