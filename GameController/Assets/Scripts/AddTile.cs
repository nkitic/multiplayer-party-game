using UnityEngine;

public class AddTile : MonoBehaviour
{
    public void MoveTile(string moveDirrection)
    {
        NetworkClientManager.SendMessageToServer(moveDirrection);
    }

    public void RotateTile()
    {
        NetworkClientManager.SendMessageToServer("RotateTile");
    }

    public void Accept()
    {
        NetworkClientManager.SendMessageToServer("Accept");
    }
}
