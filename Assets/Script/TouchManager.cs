using UnityEngine;
using UnityEngine.InputSystem;

public class TouchManager : MonoBehaviour
{
    private Camera mainCamera;
    private TouchInputActions touchActions;
    private bool isPressed = false;
    private int currentTouchId = 0;

    private void Awake()
    {
        Debug.Log("TouchManager: Awake");
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("TouchManager: Main Camera not found!");
        }

        touchActions = new TouchInputActions();
        
        Debug.Log("TouchManager: Setting up input actions");
        
        touchActions.Touch.PrimaryTouch.started += ctx => {
            Debug.Log("TouchManager: PrimaryTouch.started triggered");
            OnTouchStarted();
        };
        
        touchActions.Touch.PrimaryTouch.canceled += ctx => {
            Debug.Log("TouchManager: PrimaryTouch.canceled triggered");
            OnTouchEnded();
        };

        touchActions.Touch.PrimaryTouchPosition.performed += ctx => {
            if (isPressed)
            {
                Vector2 position = ctx.ReadValue<Vector2>();
                UpdateObjectPosition(position);
            }
        };
    }

    private void OnEnable()
    {
        Debug.Log("TouchManager: OnEnable - Enabling touch actions");
        touchActions.Enable();
        Debug.Log($"TouchManager: Actions enabled: {touchActions.Touch.enabled}");
    }

    private void OnDisable()
    {
        touchActions.Disable();
    }

    private void OnTouchStarted()
    {
        isPressed = true;
        Vector2 position = touchActions.Touch.PrimaryTouchPosition.ReadValue<Vector2>();
        Debug.Log($"TouchManager: Touch Started at position: {position}");
        HandleTouchBegan(currentTouchId, position);
    }

    private void OnTouchEnded()
    {
        if (isPressed)
        {
            isPressed = false;
            Vector2 position = touchActions.Touch.PrimaryTouchPosition.ReadValue<Vector2>();
            HandleTouchEnded(currentTouchId, position);
        }
    }

    private void UpdateObjectPosition(Vector2 screenPosition)
    {
        Vector3 worldPosition = GetWorldPosition(screenPosition);
        
        foreach (var pickable in FindObjectsOfType<PickableObject>())
        {
            if (pickable.CurrentTouchId == currentTouchId)
            {
                pickable.OnTouchMove(currentTouchId, worldPosition);
            }
        }
    }

    private void HandleTouchBegan(int touchId, Vector2 position)
    {
        Debug.Log($"TouchManager: HandleTouchBegan - Position: {position}");
        Ray ray = mainCamera.ScreenPointToRay(position);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 1f);
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log($"TouchManager: Raycast hit object: {hit.collider.gameObject.name}");
            
            var basket = hit.collider.GetComponent<Basket>();
            if (basket != null)
            {
                basket.OnTouchDown(touchId, position);
                return;
            }

            var pickable = hit.collider.GetComponent<IPickable>();
            if (pickable != null)
            {
                pickable.OnTouchPick(touchId);
            }
        }
    }

    private void HandleTouchEnded(int touchId, Vector2 position)
    {
        Debug.Log($"TouchManager: HandleTouchEnded - Position: {position}");
        foreach (var pickable in FindObjectsOfType<PickableObject>())
        {
            if (pickable.CurrentTouchId == touchId)
            {
                pickable.OnTouchDrop(touchId, position);
            }
        }
    }

    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        float zDistance = 10f;
        Vector3 worldPos = screenPosition;
        worldPos.z = zDistance;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(worldPos);
        return worldPosition;
    }
}