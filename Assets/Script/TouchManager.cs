using UnityEngine;
using UnityEngine.InputSystem;

public class TouchManager : MonoBehaviour
{
    private Camera mainCamera;
    private TouchInputActions touchActions;
    private bool isPressed = false;

    private void Awake()
    {
        Debug.Log("TouchManager: Awake");
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("TouchManager: Main Camera not found!");
        }

        touchActions = new TouchInputActions();

        // Vérification des actions
        Debug.Log("TouchManager: Setting up input actions");
        
        touchActions.Touch.PrimaryTouch.started += ctx => {
            Debug.Log("TouchManager: PrimaryTouch.started triggered");
            OnTouchStarted();
        };
        
        touchActions.Touch.PrimaryTouch.canceled += ctx => {
            Debug.Log("TouchManager: PrimaryTouch.canceled triggered");
            OnTouchEnded();
        };
    }

    private void OnEnable()
    {
        Debug.Log("TouchManager: OnEnable - Enabling touch actions");
        touchActions.Enable();
        
        // Vérification que les actions sont bien activées
        Debug.Log($"TouchManager: Actions enabled: {touchActions.Touch.enabled}");
    }

    private void OnDisable()
    {
        Debug.Log("TouchManager: OnDisable - Disabling touch actions");
        touchActions.Disable();
    }

    private void OnTouchStarted()
    {
        Debug.Log("TouchManager: OnTouchStarted called");
        isPressed = true;
        Vector2 position = touchActions.Touch.PrimaryTouchPosition.ReadValue<Vector2>();
        Debug.Log($"TouchManager: Touch position: {position}");
        HandleTouchBegan(0, position);
    }

    private void OnTouchEnded()
    {
        Debug.Log("TouchManager: OnTouchEnded called");
        if (isPressed)
        {
            isPressed = false;
            Vector2 position = touchActions.Touch.PrimaryTouchPosition.ReadValue<Vector2>();
            HandleTouchEnded(0, position);
        }
    }

    private void HandleTouchBegan(int touchId, Vector2 position)
    {
        Debug.Log($"TouchManager: HandleTouchBegan - Position: {position}");
        Ray ray = mainCamera.ScreenPointToRay(position);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 1f);
        
        int layerMask = -1; // Tous les layers
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            Debug.Log($"TouchManager: Raycast hit object: {hit.collider.gameObject.name} at position {hit.point}");
            
            var basket = hit.collider.GetComponent<Basket>();
            if (basket != null)
            {
                Debug.Log($"TouchManager: Found Basket component on {hit.collider.gameObject.name}");
                basket.OnTouchDown(touchId, position);
                return;
            }
            else
            {
                Debug.Log($"TouchManager: No Basket component found on {hit.collider.gameObject.name}");
            }

            var pickable = hit.collider.GetComponent<IPickable>();
            if (pickable != null)
            {
                Debug.Log($"TouchManager: Found Pickable component on {hit.collider.gameObject.name}");
                pickable.OnTouchPick(touchId);
            }
        }
        else
        {
            Debug.Log("TouchManager: Raycast didn't hit anything");
        }
    }

    private void HandleTouchMoved(int touchId, Vector2 position)
    {
        Vector3 worldPosition = GetWorldPosition(position);
        foreach (var pickable in FindObjectsOfType<PickableObject>())
        {
            if (pickable.CurrentTouchId == touchId)
            {
                pickable.OnTouchMove(touchId, worldPosition);
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
        Vector3 worldPos = screenPosition;
        worldPos.z = 10;
        return mainCamera.ScreenToWorldPoint(worldPos);
    }
}