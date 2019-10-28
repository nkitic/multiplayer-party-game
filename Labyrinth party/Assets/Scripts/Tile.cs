using UnityEngine;

public class Tile : MonoBehaviour
{
    public GameObject bottomWall;
    public GameObject topWall;
    public GameObject rightWall;
    public GameObject leftWall;

    [SerializeField] GameObject floor;

    public const float TILE_SIZE = 8;

    public GameObject objectOnTile = null;  

    // Use this for initialization
    void Start ()
    {
        InitalizeRandomWalls();
	}

    private void InitalizeRandomWalls()
    {
        HasWall(bottomWall);
        HasWall(topWall);
        HasWall(rightWall);
        if (bottomWall.activeInHierarchy == true && topWall.activeInHierarchy == true && rightWall.activeInHierarchy == true) return;
        HasWall(leftWall);
    }

    public static void HasWall(GameObject thisWall)
    {
        if (Random.Range(0, 2) == 1)
        {
            thisWall.SetActive(true);
        }
    }

    public void RotateTile()
    {
        var firstWallState = bottomWall.activeInHierarchy;

        bottomWall.SetActive(leftWall.activeInHierarchy);
        leftWall.SetActive(topWall.activeInHierarchy);
        topWall.SetActive(rightWall.activeInHierarchy);
        rightWall.SetActive(firstWallState);
    }

    public int GetRow()
    {
        var row = Mathf.RoundToInt(transform.position.z / TILE_SIZE);
        return row;
    }

    public int GetCol()
    {
        var col = Mathf.RoundToInt(transform.position.x / TILE_SIZE);
        return col;
    }

    public void MoveTile(GameController.Direction direction)
    {
        Vector3 transformOffset;

        if (direction == GameController.Direction.Left)
        {
            transformOffset = new Vector3(-TILE_SIZE, 0f, 0f);
        }
        else if (direction == GameController.Direction.Right)
        {
            transformOffset = new Vector3(TILE_SIZE, 0f, 0f);
        }
        else if (direction == GameController.Direction.Up)
        {
            transformOffset = new Vector3(0f, 0f, TILE_SIZE);
        }
        else
        {
            transformOffset = new Vector3(0f, 0f, -TILE_SIZE);
        }

        transform.position += transformOffset;

        if (objectOnTile != null) objectOnTile.transform.position += transformOffset;
    }

    public void ChangeMaterial(Material material)
    {
        floor.GetComponent<Renderer>().material = material;
    }
}
