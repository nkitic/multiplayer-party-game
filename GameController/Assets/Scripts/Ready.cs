using UnityEngine;
using UnityEngine.UI;

public class Ready : MonoBehaviour {

    [SerializeField] Button readyButton;

    // Use this for initialization
    void Start()
    {
        readyButton.enabled = true;
    }

    public void ImReady()
    {
        readyButton.GetComponent<Image>().color = Color.green;
        readyButton.enabled = false;

        NetworkClientManager.SendMessageToServer("Ready|" + NetworkClientManager.clientId);
    }
}
