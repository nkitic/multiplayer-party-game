using UnityEngine;

public class Player : MonoBehaviour
{
    public static Vector3 playerOffset = new Vector3(3f, 1.5f, 3f); //offset to move player in center of tile
    private int numOfCoins = 0;
    private int numOfDiamonds = 0;
    private int playerId = 0;

    public void SetPlayerId(int id)
    {
        playerId = id;
    }

    public int GetNumOfCoins()
    {
        return numOfCoins;
    }

    public int GetNumOfDiamonds()
    {
        return numOfDiamonds;
    }

    public void UpdateNumOfCoins(int coins)
    {
        numOfCoins += coins;

        NetworkGameManager.SendMessageToClient("Coins|" + numOfCoins, playerId);                
    }

    public void UpdateNumOfDiamonds(int diamond)
    {
        numOfDiamonds += diamond;

        if (numOfDiamonds == GameController.DIAMOND_WIN_COUNT)
        {
            GameController.someoneWon = true;
            GameController.winner = playerId;
        }

        NetworkGameManager.SendMessageToClient("Diamonds|" + numOfDiamonds, playerId);         
    }

    public int GetCol()
    {
        var row = Mathf.RoundToInt((transform.position.x - playerOffset.x) / Tile.TILE_SIZE);
        return row;
    }

    public int GetRow()
    {
        var col = Mathf.RoundToInt((transform.position.z - playerOffset.z) / Tile.TILE_SIZE);
        return col;
    }

    public void SetPlayerPos(int row, int col)
    {
        var transformOffset = new Vector3(Tile.TILE_SIZE * col, 0, Tile.TILE_SIZE * row);
        transform.position = transformOffset + playerOffset;
    }
}
