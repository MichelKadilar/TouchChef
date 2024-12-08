using UnityEngine;
using UnityEngine.InputSystem;

public class SliceDetector : MonoBehaviour
{
    [Header("Slice Configuration")]
    [SerializeField] private LayerMask ingredientLayer;
    [SerializeField] private float raycastDistance = 100f;
    [SerializeField] private float multiTouchDelay = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private Camera mainCamera;
    private bool wasRightMousePressed = false;
    private float lastTouchTime;
    private int touchCount;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("SliceDetector: Main Camera not found!");
        }
    }

    private void Update()
    {
        // Handle mouse input for slicing
        if (Mouse.current != null)
        {
            bool isRightMousePressed = Mouse.current.rightButton.isPressed;
            
            // Detect right mouse button press
            if (isRightMousePressed && !wasRightMousePressed)
            {
                Vector2 mousePosition = Mouse.current.position.ReadValue();
                HandleSliceAtPosition(mousePosition);
            }
            
            wasRightMousePressed = isRightMousePressed;
        }

        // Handle touch input for slicing
        if (Touchscreen.current != null)
        {
            HandleTouchSlicing();
        }
    }

    private void HandleTouchSlicing()
    {
        var touches = Touchscreen.current.touches;
        int currentTouchCount = 0;

        // Count active touches
        foreach (var touch in touches)
        {
            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                currentTouchCount++;
            }
        }

        // Check for multi-touch within the time window
        if (currentTouchCount > 0)
        {
            float currentTime = Time.time;
            if (currentTime - lastTouchTime <= multiTouchDelay)
            {
                touchCount += currentTouchCount;
                if (touchCount >= 2)
                {
                    // Get the position of the last touch for slicing
                    foreach (var touch in touches)
                    {
                        if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                        {
                            Vector2 touchPosition = touch.position.ReadValue();
                            HandleSliceAtPosition(touchPosition);
                            break;
                        }
                    }
                    touchCount = 0;
                }
            }
            else
            {
                touchCount = currentTouchCount;
            }
            lastTouchTime = currentTime;
        }
    }

    private void HandleSliceAtPosition(Vector2 position)
    {
        if (debugMode) Debug.Log($"Attempting slice at position: {position}");

        Ray ray = mainCamera.ScreenPointToRay(position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance, ingredientLayer))
        {
            GameObject clickedObject = hit.collider.gameObject;
            BaseIngredient ingredient = clickedObject.GetComponent<BaseIngredient>();

            if (ingredient != null && 
                ingredient is ISliceable sliceableIngredient && 
                ingredient.GetCurrentWorkStation()?.GetStationType() == ProcessType.Cut)
            {
                if (debugMode) Debug.Log($"Slicing ingredient: {clickedObject.name}");
                sliceableIngredient.Slice();
            }
            else
            {
                if (debugMode) Debug.Log($"Object not sliceable or not on cutting station: {clickedObject.name}");
            }
        }
    }
}