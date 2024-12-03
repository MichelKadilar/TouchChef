using UnityEngine;

public class TouchInfo
{
    public Vector2 startPosition;
    public float startTime;
    public bool isHolding;
    public GameObject targetObject;

    // Parameterless constructor (default)
    public TouchInfo()
    {
        startPosition = Vector2.zero;
        startTime = 0f;
        isHolding = false;
        targetObject = null;
    }

    // Constructor with parameters
    public TouchInfo(Vector2 startPosition, float startTime, bool isHolding, GameObject targetObject)
    {
        this.startPosition = startPosition;
        this.startTime = startTime;
        this.isHolding = isHolding;
        this.targetObject = targetObject;
    }
}