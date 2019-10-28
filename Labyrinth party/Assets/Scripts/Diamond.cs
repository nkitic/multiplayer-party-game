using UnityEngine;

public class Diamond : MonoBehaviour {

    public static Vector3 diamondOffset = new Vector3(3f, 2f, 3f); //offset to move diamond in center of tile

    public bool isAnimated = false;

    public bool isRotating = false;
    public Vector3 rotationAngle;
    public float rotationSpeed;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isAnimated)
        {
            if (isRotating)
            {
                transform.Rotate(rotationAngle * rotationSpeed * Time.deltaTime);
            }
        }
    }
}
