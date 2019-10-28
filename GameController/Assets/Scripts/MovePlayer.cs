using UnityEngine;

public class MovePlayer : MonoBehaviour {

    public void MoveTile(string moveDirrection)
    {
        NetworkClientManager.SendMessageToServer(moveDirrection);
    }

    public void Cancel()
    {
        NetworkClientManager.SendMessageToServer("Cancel");
    }

    public void Accept()
    {
        NetworkClientManager.SendMessageToServer("Accept");
    }
}
