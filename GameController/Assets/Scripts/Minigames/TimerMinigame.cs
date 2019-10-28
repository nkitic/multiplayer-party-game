using UnityEngine;
using UnityEngine.UI;

public class TimerMinigame : MonoBehaviour {

    [SerializeField] Button stopButton;

    // Use this for initialization
    void Start()
    {
        stopButton.enabled = true;
    }

    public void Stop()
    {
        stopButton.GetComponent<Image>().color = Color.red;
        stopButton.enabled = false;

        NetworkClientManager.SendMessageToServer("Stop|" + NetworkClientManager.clientId);
    }
}
