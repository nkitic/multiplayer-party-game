using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TimerMinigame : MonoBehaviour, IMessageManager
{
    private const float WAIT_SEC = 5f;
    private const float SPACE_BETWEEN_RESULTS = 100f;
    private Vector3 resultOffset;

    [SerializeField] Text stopTimeText;
    [SerializeField] Text currentTime;
    [SerializeField] Transform canvasTransform;
    [SerializeField] GameObject resultPrefab;

    private float timePassed;
    private int stopTime;

    private float[] timeStoped;
    private int playerStopCount;

    // Use this for initialization
    void Start () {
        GameController.SetNewMinigameManager(this);

        playerStopCount = 0;
        timeStoped = new float[NetworkGameManager.playerCount];

        resultOffset = new Vector3(Screen.width / 2, Screen.height / 4, 0f);

        NetworkGameManager.SendMessageToAllClients("Minigame|" + (GameController.minigameId - 1));

        StartMinigame();
    }

    // Update is called once per frame
    void Update()
    {
        timePassed += Time.deltaTime;

        if (timePassed > 1 && timePassed < 2)
        {
            currentTime.text = "1";
        }
        else if (timePassed > 2 && timePassed < 3)
        {
            currentTime.text = "2";
        }
        else if (timePassed > 3)
        {
            currentTime.text = "x";
        }
    }

    public void ProcessNetworkMessages(string message)
    {
        string[] messageParts = message.Split('|');

        if (messageParts[0] == "Stop")
        {
            timeStoped[System.Int32.Parse(messageParts[1])] = timePassed;

            playerStopCount++;
            if (playerStopCount == NetworkGameManager.playerCount)
                EndMinigame();
        }
    }

    public void StartMinigame()
    {
        stopTime = Random.Range(5, 10);

        stopTimeText.text = "Stop time at: " + stopTime;

        timePassed = 0;
    }

    public void EndMinigame()
    {
        NetworkGameManager.SendMessageToAllClients("Loading");

        ShowTimeResluts();
        SaveMinigameResults();
        StartCoroutine(LoadEndMingame());
    }

    public void ShowTimeResluts()
    {
        var transformOffset = new Vector3(0f, 0f, 0f);

        if (NetworkGameManager.playerCount == 2)     
        {
            transformOffset.x = -SPACE_BETWEEN_RESULTS / 2;
        }
        else if(NetworkGameManager.playerCount == 3)    
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

    public void SaveMinigameResults()               //can be used for other minigame results
    {
        float[] sortedTimeResults = new float[NetworkGameManager.playerCount];
        int[] minigameEndResults = new int[NetworkGameManager.playerCount];

        for (var i = 0; i < NetworkGameManager.playerCount; i++)
            sortedTimeResults[i] = timeStoped[i];

        for (var i = 0 ; i < NetworkGameManager.playerCount; i++)
        {
            float minResult = Mathf.Abs(sortedTimeResults[i] - stopTime);
            int minIndex = i;

            for (var j = i + 1; j < NetworkGameManager.playerCount; j++)
            {
                if(Mathf.Abs(sortedTimeResults[j] - stopTime) < minResult)
                {
                    minResult = Mathf.Abs(sortedTimeResults[j] - stopTime);
                    minIndex = j;
                }
            }

            var tempResult = sortedTimeResults[i];
            sortedTimeResults[i] = sortedTimeResults[minIndex];
            sortedTimeResults[minIndex] = tempResult;
        }

        for (var i = 0; i < NetworkGameManager.playerCount; i++)
        {
            for (var j = 0; j < NetworkGameManager.playerCount; j++)
            {
                if (timeStoped[i] == sortedTimeResults[j])
                {
                    minigameEndResults[i] = j;
                }
            }
        }

        GameController.minigameResults = minigameEndResults;
    }

    private void InstantiateResult(int playerId, Vector3 transformOffset)
    {
        transformOffset += resultOffset;
        var resultObject = Instantiate(resultPrefab, transformOffset, Quaternion.identity, canvasTransform);

        var resultText = resultObject.transform.Find("Result Text").gameObject.GetComponent<Text>();
        resultText.text = timeStoped[playerId].ToString("F2");

        var playerImage = resultObject.transform.Find("Player Image").gameObject.GetComponent<Image>();
        playerImage.color = PlayingAreaController.players[playerId].GetComponent<Renderer>().material.color;
    }

    private IEnumerator LoadEndMingame()
    {
        yield return new WaitForSeconds(WAIT_SEC);
 
        SceneManager.UnloadSceneAsync(GameController.minigameId);
        SceneManager.LoadScene(3, LoadSceneMode.Additive);
    }

}
