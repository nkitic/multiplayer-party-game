using UnityEngine;

public class PlayingAreaController : MonoBehaviour
{
    #region Fields

    [Header("Playing area")]
    [SerializeField] GameObject tilePrefab;
    [SerializeField] Transform parent;
    [SerializeField] int playingAreaSize = 9; 

    [Header("Players")]
    [SerializeField] GameObject playerPrefab;
    private int numOfPlayers;

    [Header("Diamond")]
    [SerializeField] GameObject diamondPrefab;

    public static GameObject[,] tiles;
    public static GameObject[] players;

    private Color[] playerColors = { Color.red, Color.blue, Color.green, Color.gray };

    #endregion

    #region Start / Update

    // Use this for initialization
    void Start ()
    {
        numOfPlayers = NetworkGameManager.playerCount;
        players = new GameObject[numOfPlayers];
        tiles = new GameObject[playingAreaSize, playingAreaSize];

        CreatePlayingArea();
        SpawnDiamond(Mathf.RoundToInt(playingAreaSize / 2), Mathf.RoundToInt(playingAreaSize / 2));
        SpawnPlayers();
	}

    #endregion

    #region Initialization

    private void CreatePlayingArea()
    {
        var transformOffset = new Vector3(0f, 0f, 0f);

        for(var row = 0; row < playingAreaSize; row++)
        {
            for (var col = 0; col < playingAreaSize;col++)
            {
                tiles[row, col] = Instantiate(tilePrefab, transformOffset, Quaternion.identity);
                tiles[row, col].transform.parent = parent;
                transformOffset.x += Tile.TILE_SIZE;
            }

            transformOffset.z += Tile.TILE_SIZE;
            transformOffset.x = 0;
        }
    }

    private void SpawnPlayers()
    {
        var transformOffset = Player.playerOffset;          //todo: use setplayerpos()
        
        players[0] = InstantiatePlayer(transformOffset, playerColors[0]);
        tiles[players[0].GetComponent<Player>().GetRow(), players[0].GetComponent<Player>().GetCol()].GetComponent<Tile>().objectOnTile = players[0];
        players[0].GetComponent<Player>().SetPlayerId(0);

        if (numOfPlayers > 1)
        { 
            transformOffset.x += Tile.TILE_SIZE * (playingAreaSize - 1);
            transformOffset.z += Tile.TILE_SIZE * (playingAreaSize - 1);
            players[1] = InstantiatePlayer(transformOffset, playerColors[1]);
            tiles[players[1].GetComponent<Player>().GetRow(), players[1].GetComponent<Player>().GetCol()].GetComponent<Tile>().objectOnTile = players[1];
            players[1].GetComponent<Player>().SetPlayerId(1);
        }
        if (numOfPlayers > 2)
        {
            transformOffset.z -= Tile.TILE_SIZE * (playingAreaSize - 1);
            players[2] = InstantiatePlayer(transformOffset, playerColors[2]);
            tiles[players[2].GetComponent<Player>().GetRow(), players[2].GetComponent<Player>().GetCol()].GetComponent<Tile>().objectOnTile = players[2];
            players[2].GetComponent<Player>().SetPlayerId(2);
        }

        if(numOfPlayers > 3)
        {
            transformOffset.x -= Tile.TILE_SIZE * (playingAreaSize - 1);
            transformOffset.z += Tile.TILE_SIZE * (playingAreaSize - 1);
            players[3] = InstantiatePlayer(transformOffset, playerColors[3]);
            tiles[players[3].GetComponent<Player>().GetRow(), players[3].GetComponent<Player>().GetCol()].GetComponent<Tile>().objectOnTile = players[3];
            players[3].GetComponent<Player>().SetPlayerId(3);
        }
    }

    private GameObject InstantiatePlayer(Vector3 transform, Color playerColor)
    {
        var newPlayer = Instantiate(playerPrefab, transform, Quaternion.identity);
        
        newPlayer.transform.parent = parent;

        var newPlayerMaterial = newPlayer.GetComponent<Renderer>().material;
        newPlayerMaterial.color = playerColor;

        return newPlayer;
    }

    private void SpawnDiamond(int row, int col)
    {
        var transformOffset = Diamond.diamondOffset;

        transformOffset.x += Tile.TILE_SIZE * col;
        transformOffset.z += Tile.TILE_SIZE * row;

        var diamond = Instantiate(diamondPrefab, transformOffset, Quaternion.Euler(new Vector3(-90f, 0f, 0f)));

        diamond.transform.parent = parent;

        tiles[row, col].GetComponent<Tile>().objectOnTile = diamond;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Check if connected with neighbour tile. Two tiles are conneceted if there is no wall between them.
    /// </summary>
    /// <param name="neighbourRow">Row of neighbour tile</param>
    /// <param name="neighbourCol">Col of neighbour tile</param>
    /// <param name="direction">Represents which neighbour</param>
    /// <returns>Returns tile if they are conneceted, else returns null</returns>
    public static Tile CheckNeighbourConnection(int neighbourRow, int neighbourCol, GameController.Direction direction)
    {
        var neighbourTile = tiles[neighbourRow, neighbourCol].GetComponent<Tile>();
        if (direction == GameController.Direction.Left)
        {
            if (neighbourTile.rightWall.activeInHierarchy)
                return null;
        }
        else if(direction == GameController.Direction.Right)
        {
            if (neighbourTile.leftWall.activeInHierarchy)
                return null;
        }
        else if (direction == GameController.Direction.Up)
        {
            if (neighbourTile.bottomWall.activeInHierarchy)
                return null;
        }
        else if (direction == GameController.Direction.Down)
        {
            if (neighbourTile.topWall.activeInHierarchy)
                return null;
        }

       return neighbourTile;
    }

    public static Tile GetTile(int tileRow, int tileCol)
    {
        return tiles[tileRow, tileCol].GetComponent<Tile>();
    }

    #endregion
}
