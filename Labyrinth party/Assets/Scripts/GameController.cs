using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour, IMessageManager
{
    #region Fields

    private const int ADD_TILE_COST = 3;
    private const int MOVE_ONE_TILE_COST = 1;
    private const float LOAD_SCENE_WAIT_SEC = 1.5f;
    public const int DIAMOND_WIN_COUNT = 1;

    private enum GameState { NewTurn, OptionMenu, AddTile, MovePlayer, Minigame, Transitioning };
    private static GameState currentGameState = GameState.NewTurn;

    private int numOfPlayers;
    public static int playerTrun;
    private static Player currentPlayer;

    public enum Direction { Left, Right, Up, Down, None };

    [SerializeField] GameObject tilePrefab;
    [SerializeField] Transform parent;
    private int playingAreaSize;
    private GameObject newTile;

    private List<Tile> movingPathTiles = new List<Tile>();
    private Tile leftNeighbour = null, rightNeighbour = null, topNeighbour = null, bottomNeighbour = null;
    [SerializeField] Material defaultMaterial;
    [SerializeField] Material selectedMaterial;
    [SerializeField] Material connectedNeighbourMaterial;

    public static IMessageManager minigameManager;
    public static int minigameId;
    public static int[] minigameResults = new int[NetworkGameManager.playerCount];      //keeps track of player position in minagame
    public static int[] minigameRewards = { 5, 3, 1, 0 };       //how much coins gets each player

    [SerializeField] Text diceRollText;
    [SerializeField] Text playerTurnText;

    //public static int[] gameResults = new int[NetworkGameManager.playerCount];
    public static bool someoneWon = false;
    public static int winner;

    #endregion

    #region Start / ProcessNetworkMessages

    // Use this for initialization
    void Start()
    {
        NetworkGameManager.SetNewGameManager(this);

        playingAreaSize = Mathf.RoundToInt(Mathf.Sqrt(PlayingAreaController.tiles.Length));

        playerTrun = 0;
        numOfPlayers = NetworkGameManager.playerCount;

        someoneWon = false;

        playerTurnText.text = "Player" + (playerTrun + 1);
        StartNewTurn();
    }

    public void ProcessNetworkMessages(string message)
    {
        Debug.Log(message);
        if (currentGameState == GameState.OptionMenu)
        {
            ProcessMessageOptionMenu(message);
        }
        else if (currentGameState == GameState.AddTile)
        {
            ProcessMessageAddTile(message);
        }
        else if (currentGameState == GameState.MovePlayer)
        {
            ProcessMessageMovePlayer(message);
        }
        else if (currentGameState == GameState.Minigame)
        {
            minigameManager.ProcessNetworkMessages(message);
        }
    }

    #endregion

    #region Player turn

    private bool CheckIfRoundEnded()
    {
        if (playerTrun == numOfPlayers)
        {
            playerTrun = 0;
            return true;
        }
        return false;
    }

    private static void StartNewTurn()
    {
        currentGameState = GameState.Transitioning;

        currentPlayer = PlayingAreaController.players[playerTrun].GetComponent<Player>();

        currentGameState = GameState.OptionMenu;

        NetworkGameManager.SendMessageToClient("StartTurn", playerTrun);

    }

    private void ProcessMessageOptionMenu(string message)
    {
        if (message == "ThrowDice")
        {
            ThrowDice();
        }
        else if (message == "TurnDone")
        {
            EndTrun();
        }
        else if (message == "AddTile")
        {
            OpenAddTile();         
        }
        else if (message == "MovePlayer")
        {
            OpenMovePlayer();
        }
    }

    private void ProcessMessageAddTile(string message)
    {
        if (message == "MoveLeft")
        {
            MoveNewTile(Direction.Left);
        }
        else if (message == "MoveRight")
        {
            MoveNewTile(Direction.Right);
        }
        else if (message == "MoveUp")
        {
            MoveNewTile(Direction.Up);
        }
        else if (message == "MoveDown")
        {
            MoveNewTile(Direction.Down);
        }
        else if (message == "RotateTile")
        {
            RotateNewTile();
        }
        else if (message == "Accept")
        {
            if (InsertNewTile())
                BackToOptionMenu(); // somtimes tile cannot be inserted
        }
    }

    private void ProcessMessageMovePlayer(string message)
    {
        if (message == "Cancel")
        {
            ResetMovePlayer();
            BackToOptionMenu();
        }
        else if (message == "MoveLeft")
        {
            ProcessMoveButton(leftNeighbour);
        }
        else if (message == "MoveRight")
        {
            ProcessMoveButton(rightNeighbour);
        }
        else if (message == "MoveUp")
        {
            ProcessMoveButton(topNeighbour);
        }
        else if (message == "MoveDown")
        {
            ProcessMoveButton(bottomNeighbour);
        }
        else if (message == "Accept")
        {
            if(MovePlayer())
            {
                PayMovingCost();
                ResetMovePlayer();
                BackToOptionMenu();
                EndTrun();
            }
        }
    }

    private void ThrowDice()
    {
        currentGameState = GameState.Transitioning;

        var diceNumber = Random.Range(1, 7);

        diceRollText.text = "Thrown: " + diceNumber;

        currentPlayer.UpdateNumOfCoins(diceNumber);

        currentGameState = GameState.OptionMenu;
    }

    private void BackToOptionMenu()
    {
        currentGameState = GameState.Transitioning;

        NetworkGameManager.SendMessageToClient("Back", playerTrun);

        currentGameState = GameState.OptionMenu;
    }

    private void EndTrun()
    {
        currentGameState = GameState.Transitioning;
        diceRollText.text = "Thrown: ";
        NetworkGameManager.SendMessageToClient("EndTurn", playerTrun);
        playerTrun++;

        if(someoneWon)
        {
            StartCoroutine(EndGame());
        }
        else if (CheckIfRoundEnded())
        {
            StartCoroutine(StartMinigame());
        }
        else
        {
            currentGameState = GameState.NewTurn;
            playerTurnText.text = "Player" + (playerTrun + 1);
            StartNewTurn();
        }
    }

    #endregion

    #region Move player

    private void OpenMovePlayer()
    {
        currentGameState = GameState.Transitioning;

        var currentRow = currentPlayer.GetRow();
        var currentCol = currentPlayer.GetCol();
        AddTileToMovingPath(PlayingAreaController.GetTile(currentRow, currentCol));  //add first tile to moving path
        ShowNextPossibleMoves(currentRow, currentCol);

        currentGameState = GameState.MovePlayer;
    }

    private void ShowNextPossibleMoves(int currentRow, int currentCol)
    {
        var currentTile = PlayingAreaController.GetTile(currentRow, currentCol);

        if (!currentTile.leftWall.activeInHierarchy && currentTile.GetCol() != 0)       
        {
            leftNeighbour = PlayingAreaController.CheckNeighbourConnection(currentRow, currentCol - 1, Direction.Left);
            if (!CheckIfTileInPath(PlayingAreaController.GetTile(currentRow, currentCol - 1)))
            {
                ChangeTileMaterial(leftNeighbour, connectedNeighbourMaterial);                
            }
            else
            {
                if (leftNeighbour != movingPathTiles[movingPathTiles.Count - 2])
                    leftNeighbour = null;
            }
        }
        if (!currentTile.rightWall.activeInHierarchy && currentTile.GetCol() != playingAreaSize - 1)
        {
            rightNeighbour = PlayingAreaController.CheckNeighbourConnection(currentRow, currentCol + 1, Direction.Right);
            if (!CheckIfTileInPath(PlayingAreaController.GetTile(currentRow, currentCol + 1)))
            {
                ChangeTileMaterial(rightNeighbour, connectedNeighbourMaterial);               
            }
            else
            {
                if (rightNeighbour != movingPathTiles[movingPathTiles.Count - 2])
                    rightNeighbour = null;
            }
        }
        if (!currentTile.topWall.activeInHierarchy && currentTile.GetRow() != playingAreaSize - 1)
        {
            topNeighbour = PlayingAreaController.CheckNeighbourConnection(currentRow + 1, currentCol, Direction.Up);
            if (!CheckIfTileInPath(PlayingAreaController.GetTile(currentRow + 1, currentCol)))
            {
                ChangeTileMaterial(topNeighbour, connectedNeighbourMaterial);
            }
            else
            {
                if (topNeighbour != movingPathTiles[movingPathTiles.Count - 2])
                    topNeighbour = null;
            }
        }
        if (!currentTile.bottomWall.activeInHierarchy && currentTile.GetRow() != 0)
        {
            bottomNeighbour = PlayingAreaController.CheckNeighbourConnection(currentRow - 1, currentCol, Direction.Down);
            if (!CheckIfTileInPath(PlayingAreaController.GetTile(currentRow - 1, currentCol)))
            {
                ChangeTileMaterial(bottomNeighbour, connectedNeighbourMaterial);
            }
            else
            {
                if (bottomNeighbour != movingPathTiles[movingPathTiles.Count - 2])
                    bottomNeighbour = null;
            }
        }

        CheckIfEnoughCoins();
    }

    private void CheckIfEnoughCoins()
    {
        if (currentPlayer.GetNumOfCoins() <= movingPathTiles.Count - 1)   //if player cannot pay any more moves dont show next possible moves
        {
            if(!CheckIfTileInPath(leftNeighbour))
            {
                ChangeTileMaterial(leftNeighbour, defaultMaterial);
                leftNeighbour = null;
            }
            if (!CheckIfTileInPath(rightNeighbour))
            {
                ChangeTileMaterial(rightNeighbour, defaultMaterial);
                rightNeighbour = null;
            }
            if (!CheckIfTileInPath(topNeighbour))
            {
                ChangeTileMaterial(topNeighbour, defaultMaterial);
                topNeighbour = null;
            }
            if (!CheckIfTileInPath(bottomNeighbour))
            {
                ChangeTileMaterial(bottomNeighbour, defaultMaterial);
                bottomNeighbour = null;
            }
        }
    }

    private bool CheckIfTileInPath(Tile tileToAdd)
    {
        foreach(var tile in movingPathTiles)
        {
            if (tile == tileToAdd)
                return true;
        }
        return false;
    }

    private void ProcessMoveButton(Tile tile)
    {
        if (tile != null)
        {
            if (movingPathTiles.Count <= 1)
            {
                AddTileToMovingPath(tile);
            }
            else
            {
                if (tile != movingPathTiles[movingPathTiles.Count - 2])  //check if neighbour is in list before him
                    AddTileToMovingPath(tile);
                else
                    RemoveTileFromMovingPath(movingPathTiles[movingPathTiles.Count - 1]);
            }
        }
    }

    private void AddTileToMovingPath(Tile tile)
    {
        UnselectPossibleMoves();
        tile.ChangeMaterial(selectedMaterial);
        movingPathTiles.Add(tile);
        ShowNextPossibleMoves(tile.GetRow(), tile.GetCol());
    }

    private void RemoveTileFromMovingPath(Tile tile)
    {
        UnselectPossibleMoves();
        tile.ChangeMaterial(defaultMaterial);
        movingPathTiles.Remove(tile);
        ShowNextPossibleMoves(movingPathTiles[movingPathTiles.Count - 1].GetRow(), movingPathTiles[movingPathTiles.Count - 1].GetCol());
    }

    private void UnselectPossibleMoves()
    {
        ChangeTileMaterial(leftNeighbour, defaultMaterial);
        leftNeighbour = null;

        ChangeTileMaterial(rightNeighbour, defaultMaterial);
        rightNeighbour = null;

        ChangeTileMaterial(topNeighbour, defaultMaterial);
        topNeighbour = null;

        ChangeTileMaterial(bottomNeighbour, defaultMaterial);
        bottomNeighbour = null;
    }

    private void ChangeTileMaterial(Tile tile, Material material)
    {
        if (tile != null && !CheckIfTileInPath(tile))
            tile.ChangeMaterial(material);
    }

    private void ResetMovePlayer()
    {
        leftNeighbour = null;
        rightNeighbour = null;
        topNeighbour = null;
        bottomNeighbour = null;
        movingPathTiles.Clear();
        UnselectAllTiles();
    }

    private void UnselectAllTiles()
    {
        var tiles = PlayingAreaController.tiles;

        foreach (var tile in tiles)
            ChangeTileMaterial(tile.GetComponent<Tile>(), defaultMaterial);
    }

    private bool MovePlayer()
    {
        if (movingPathTiles.Count <= 1)
            return false;

        var oldPlayerTile = PlayingAreaController.tiles[currentPlayer.GetRow(), currentPlayer.GetCol()].GetComponent<Tile>();
        oldPlayerTile.objectOnTile = null;

        var newPlayerTile = PlayingAreaController.tiles[movingPathTiles[movingPathTiles.Count - 1].GetRow(), movingPathTiles[movingPathTiles.Count - 1].GetCol()].GetComponent<Tile>();

        if (newPlayerTile.objectOnTile != null)
        {
            if (newPlayerTile.objectOnTile.GetComponent<Diamond>() != null)
            {
                currentPlayer.UpdateNumOfDiamonds(1);

                Destroy(newPlayerTile.objectOnTile);
            }
            else if (newPlayerTile.objectOnTile.GetComponent<Player>() != null)
            {
                NetworkGameManager.SendMessageToClient("TileOccupied", playerTrun);
                return false;
            }
        }

        currentPlayer.SetPlayerPos(movingPathTiles[movingPathTiles.Count - 1].GetRow(), movingPathTiles[movingPathTiles.Count - 1].GetCol());
        newPlayerTile.objectOnTile = currentPlayer.gameObject;

        return true;
    }

    private void PayMovingCost()
    {
        var movingCost = -((movingPathTiles.Count - 1) * MOVE_ONE_TILE_COST);
        currentPlayer.UpdateNumOfCoins(movingCost);
    }

    #endregion

    #region Add tile

    private void OpenAddTile()
    {
        currentGameState = GameState.Transitioning;

        currentPlayer.UpdateNumOfCoins(-ADD_TILE_COST);
        SpawnNewTile();

        currentGameState = GameState.AddTile;
    }

    private void SpawnNewTile()
    {
        var transformOffset = new Vector3(0f, 0f, -Tile.TILE_SIZE);

        newTile = Instantiate(tilePrefab, transformOffset, Quaternion.identity); //spawn tile on pos row = -1, col = 0
        newTile.transform.parent = parent;
    }

    private void MoveNewTile(Direction direction)
    {
        var tileCol = newTile.GetComponent<Tile>().GetCol();
        var tileRow = newTile.GetComponent<Tile>().GetRow();
        if (direction == Direction.Left && (tileRow == -1 || tileRow == playingAreaSize))
        {
            //edge cases
            if (tileCol == 0 && tileRow == -1)
            {
                newTile.GetComponent<Tile>().MoveTile(Direction.Up);
            }
            if (tileCol == 0 && tileRow == playingAreaSize)
            {
                newTile.GetComponent<Tile>().MoveTile(Direction.Down);
            }

            newTile.GetComponent<Tile>().MoveTile(direction);
        }
        else if (direction == Direction.Right && (tileRow == -1 || tileRow == playingAreaSize))
        {
            //edge cases
            if (tileCol == playingAreaSize - 1 && tileRow == playingAreaSize)
            {
                newTile.GetComponent<Tile>().MoveTile(Direction.Down);
            }
            if (tileCol == playingAreaSize - 1 && tileRow == -1)
            {
                newTile.GetComponent<Tile>().MoveTile(Direction.Up);
            }

            newTile.GetComponent<Tile>().MoveTile(direction);
        }
        else if (direction == Direction.Up && (tileCol == -1 || tileCol == playingAreaSize))
        {
            //edge cases
            if (tileRow == playingAreaSize - 1 && tileCol == -1)
            {
                newTile.GetComponent<Tile>().MoveTile(Direction.Right);
            }
            if (tileRow == playingAreaSize - 1 && tileCol == playingAreaSize)
            {
                newTile.GetComponent<Tile>().MoveTile(Direction.Left);
            }

            newTile.GetComponent<Tile>().MoveTile(direction);
        }
        else if (direction == Direction.Down && (tileCol == -1 || tileCol == playingAreaSize))
        {
            //edge cases
            if (tileRow == 0 && tileCol == playingAreaSize)
            {
                newTile.GetComponent<Tile>().MoveTile(Direction.Left);
            }
            if (tileRow == 0 && tileCol == -1)
            {
                newTile.GetComponent<Tile>().MoveTile(Direction.Right);
            }

            newTile.GetComponent<Tile>().MoveTile(direction);
        }
    }

    private void RotateNewTile()
    {
        newTile.GetComponent<Tile>().RotateTile();
    }

    private void DestroyTile(GameObject tile)
    {
        Destroy(tile);
    }

    /// <summary>
    /// Inserts tile in col/row
    /// Tile cannot be inserted if edge tile contains game object
    /// </summary>
    /// <returns>True if tile is inseted, False if tile cannot be inserted</returns>
    private bool InsertNewTile()
    {
        currentGameState = GameState.Transitioning;

        var tilesMoved = StartMoveAllTilesInRowCol();

        if (tilesMoved)
        {
            return true;
        }
        else
        {
            Debug.Log("cannot add tile");
            currentGameState = GameState.AddTile;
            return false;
        }
    }

    private bool StartMoveAllTilesInRowCol()
    {
        GameObject tileToDestroy = null;
        Direction direction = Direction.None;
        int newTileRow = 0, newTileCol = 0;

        var row = newTile.GetComponent<Tile>().GetRow();
        var col = newTile.GetComponent<Tile>().GetCol();

        if (row == playingAreaSize)
        {
            tileToDestroy = PlayingAreaController.tiles[0, col];
            if (tileToDestroy.GetComponent<Tile>().objectOnTile == null)
                direction = Direction.Down;

            newTileRow = playingAreaSize - 1;
            newTileCol = col;
        }
        else if (row == -1)
        {
            tileToDestroy = PlayingAreaController.tiles[playingAreaSize - 1, col];
            if (tileToDestroy.GetComponent<Tile>().objectOnTile == null)
                direction = Direction.Up;

            newTileRow = 0;
            newTileCol = col;
        }
        else if (col == playingAreaSize)
        {
            tileToDestroy = PlayingAreaController.tiles[row, 0];
            if (tileToDestroy.GetComponent<Tile>().objectOnTile == null)
                direction = Direction.Left;

            newTileRow = row;
            newTileCol = playingAreaSize - 1;
        }
        else if (col == -1)
        {
            tileToDestroy = PlayingAreaController.tiles[row, playingAreaSize - 1];
            if (tileToDestroy.GetComponent<Tile>().objectOnTile == null)
                direction = Direction.Right;

            newTileRow = row;
            newTileCol = 0;
        }

        if (direction == Direction.None)
            return false;

        DestroyTile(tileToDestroy);
        MoveAllTilesInRowCol(direction, col, row);
        MoveNewTile(direction, newTileRow, newTileCol);

        return true;
    }

    private void MoveNewTile(Direction direction, int newTileRow, int newTileCol)
    {
        PlayingAreaController.tiles[newTileRow, newTileCol] = newTile;
        PlayingAreaController.tiles[newTileRow, newTileCol].GetComponent<Tile>().MoveTile(direction);
    }

    private void MoveAllTilesInRowCol(Direction direction, int col, int row)
    {
        var tiles = PlayingAreaController.tiles;

        if (direction == Direction.Up || direction == Direction.Right)
        {
            for (var i = playingAreaSize - 1; i > 0; i--) //move tiles
            {
                if (direction == Direction.Up)
                {
                    tiles[i, col] = tiles[i - 1, col];
                    tiles[i, col].GetComponent<Tile>().MoveTile(direction);
                }
                else
                {
                    tiles[row, i] = tiles[row, i - 1];
                    tiles[row, i].GetComponent<Tile>().MoveTile(direction);

                }
            }
        }
        else
        {
            for (var i = 0; i < playingAreaSize - 1; i++) //move tiles
            {
                if (direction == Direction.Down)
                {
                    tiles[i, col] = tiles[i + 1, col];
                    tiles[i, col].GetComponent<Tile>().MoveTile(direction);
                }
                else
                {
                    tiles[row, i] = tiles[row, i + 1];
                    tiles[row, i].GetComponent<Tile>().MoveTile(direction);

                }
            }
        }

    }

    #endregion

    #region Minigames

    private IEnumerator StartMinigame()
    {
        currentGameState = GameState.Transitioning;

        yield return new WaitForSeconds(LOAD_SCENE_WAIT_SEC);
        
        currentGameState = GameState.Minigame;
        playerTurnText.text = "Player" + (playerTrun + 1);
        SceneManager.LoadScene(2, LoadSceneMode.Additive);
    }

    public static void EndMinigame()
    {
        currentGameState = GameState.Transitioning;
        EnablePlayers();                            //TODO: while in minigame player objects gets disabled - find out why!
        GiveMinigameRewards();
        currentGameState = GameState.NewTurn;

        StartNewTurn();
    }

    private static void EnablePlayers()
    {
        foreach(var player in PlayingAreaController.players)
        {
            player.gameObject.SetActive(true);
        }
    }

    public static void GiveMinigameRewards()
    {
        for(var playerId = 0; playerId < NetworkGameManager.playerCount; playerId++)
        {
            PlayingAreaController.players[playerId].GetComponent<Player>().UpdateNumOfCoins(minigameRewards[minigameResults[playerId]]);
        }
    }

    public static void SetNewMinigameManager(IMessageManager newMinigameManager)
    {
        minigameManager = newMinigameManager;
    }

    #endregion

    #region EndGame

    private IEnumerator EndGame()
    {

        yield return new WaitForSeconds(LOAD_SCENE_WAIT_SEC);

        SceneManager.LoadScene(4, LoadSceneMode.Additive);
    }

    #endregion
}
